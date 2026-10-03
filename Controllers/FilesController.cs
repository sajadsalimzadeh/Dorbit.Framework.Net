using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Attachments;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Repositories;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Dorbit.Framework.Controllers;

[ApiExplorerSettings(GroupName = "framework")]
[Route("Framework/[controller]")]
public class FilesController(
    IMemoryCache memoryCache,
    IOptions<ConfigFile> configFileOptions,
    IIdentityService identityService,
    AttachmentRepository attachmentRepository) : BaseController
{
    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".htm", ".svg", ".js", ".mjs", ".xml", ".xhtml", ".shtml",
        ".aspx", ".asp", ".php", ".exe", ".dll", ".bat", ".cmd", ".ps1",
        ".hta", ".vbs", ".wsf", ".scr", ".msi", ".com", ".jar", ".cshtml"
    };

    private static readonly HashSet<string> InlineExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp", ".pdf", ".txt", ".csv"
    };

    private string GetFilePath(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new OperationException(FrameworkErrors.FilePathIsInvalid);

        var safeName = Path.GetFileName(filename);
        if (!string.Equals(safeName, filename, StringComparison.Ordinal) ||
            safeName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new OperationException(FrameworkErrors.FilePathIsInvalid);

        var basePath = Path.GetFullPath(configFileOptions.Value.BasePath);
        Directory.CreateDirectory(basePath);
        var filePath = Path.GetFullPath(Path.Combine(basePath, safeName));
        var relative = Path.GetRelativePath(basePath, filePath);
        if (relative.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(relative))
            throw new OperationException(FrameworkErrors.FilePathIsInvalid);

        return filePath;
    }

    private class FileDto
    {
        public Attachment Attachment { get; set; }
        public DateTimeOffset? LastModifyTime { get; set; }
        public byte[] Content { get; set; }
    }

    private bool ValidateAccess(Attachment attachment)
    {
        if (!attachment.IsPrivate)
            return true;

        var identity = identityService.Identity;
        if (identity?.User is not null && attachment.UserId == identity.User.GetId())
            return true;

        if (attachment.Access.IsNotNullOrEmpty() && identity is not null && identity.HasAccess(attachment.Access))
            return true;

        if (identity?.User is not null && attachment.UserIds != null && attachment.UserIds.Contains(identity.User.GetId()))
            return true;

        if (attachment.AccessTokens != null &&
            Request.Headers.TryGetValue("FileAuthorization", out var accessToken) &&
            attachment.AccessTokens.Contains(accessToken.ToString()))
            return true;

        return false;
    }

    private void ApplyFileResponseHeaders(FileDto fileDto)
    {
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["Content-Security-Policy"] = "default-src 'none'; sandbox";
        var isPublic = fileDto.Attachment is { IsPrivate: false };
        Response.Headers["Cache-Control"] = isPublic ? "public,max-age=86400" : "private,no-store";
    }

    private IActionResult FileResult(FileDto fileDto, string filename, bool download)
    {
        ApplyFileResponseHeaders(fileDto);
        var extension = Path.GetExtension(filename);
        if (download || !InlineExtensions.Contains(extension))
            return File(fileDto.Content, "application/octet-stream", Path.GetFileName(filename));

        return File(fileDto.Content, MimeTypeUtil.GetMimeTypeByFilename(filename));
    }

    private async Task<FileDto> GetFileAsync(string filename)
    {
        var key = "File-" + filename;
        var fileDto = await memoryCache.GetValueWithLockAsync(key, async () =>
        {
            var dto = new FileDto();
            dto.Attachment = await attachmentRepository.FirstOrDefaultAsync(x => x.Filename == filename);
            var filePath = GetFilePath(filename);
            var fileInfo = new FileInfo(filePath);
            if (!System.IO.File.Exists(filePath)) throw new FileNotFoundException();
            dto.LastModifyTime = fileInfo.LastWriteTime;
            dto.Content = await System.IO.File.ReadAllBytesAsync(filePath);
            return dto;
        }, TimeSpan.FromMinutes(5));
        if (fileDto.Attachment != null && !ValidateAccess(fileDto.Attachment))
            throw new UnauthorizedAccessException();

        return fileDto;
    }

    [HttpPost, Auth, AntiDos(AntiDosAttribute.DurationType.Hour, 20)]
    public async Task<QueryResult<string>> UploadAsync([FromForm] AttachmentUploadPrivateRequest request)
    {
        var file = Request.Form.Files[0];
        if (file.Length > configFileOptions.Value.MaxSize)
        {
            var hasAccess = false;
            if (configFileOptions.Value.MaxSizeAccessibility is not null)
            {
                foreach (var access in configFileOptions.Value.MaxSizeAccessibility)
                {
                    if (identityService.Identity.HasAccess(access.Key) && file.Length < access.Value)
                    {
                        hasAccess = true;
                        break;
                    }
                }
            }

            if (!hasAccess)
                throw new OperationException(FrameworkErrors.MaxSizeOverflow)
                {
                    Data =
                    {
                        { "MaxSize", configFileOptions.Value.MaxSize }
                    }
                };
        }


        var ext = Path.GetExtension(file.FileName);
        if (BlockedExtensions.Contains(ext))
            throw new OperationException(FrameworkErrors.FileTypeIsNotAllowed);

        var filename = Guid.NewGuid() + ext;
        var filePath = GetFilePath(filename);
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());

        await attachmentRepository.InsertAsync(new Attachment()
        {
            UserId = GetUserId(),
            IsPrivate = true,
            Filename = filename,
            Size = file.Length,
            Access = request?.Access,
        });

        return new QueryResult<string>(filename);
    }

    [HttpGet("{filename}"), Auth(IsOptional = true)]
    public async Task<IActionResult> GetAsync([FromRoute] string filename)
    {
        var fileDto = await GetFileAsync(filename);
        return FileResult(fileDto, filename, download: false);
    }

    [HttpGet("{filename}/Download"), Auth(IsOptional = true)]
    public async Task<IActionResult> DownloadAsync([FromRoute] string filename)
    {
        var fileDto = await GetFileAsync(filename);
        return FileResult(fileDto, filename, download: true);
    }

    [HttpGet("{filename}/Info"), Auth]
    public async Task<QueryResult<Attachment>> GetInfoAsync([FromRoute] string filename)
    {
        var attachment = await attachmentRepository.FirstOrDefaultAsync(x => x.Filename == filename)
                         ?? throw new OperationException(FrameworkErrors.EntityNotFound);

        if (attachment.IsPrivate && !ValidateAccess(attachment))
            throw new UnauthorizedAccessException();

        return attachment.ToQueryResult();
    }

    [HttpPatch("{filename}/Info"), Auth]
    public async Task<QueryResult<Attachment>> PatchInfoAsync([FromRoute] string filename, [FromBody] JsonElement request)
    {
        var attachment = await attachmentRepository.FirstOrDefaultAsync(x => x.Filename == filename)
                         ?? throw new OperationException(FrameworkErrors.EntityNotFound);

        if (identityService.Identity.User.GetId() != attachment.UserId)
            throw new UnauthorizedAccessException();

        attachment = await attachmentRepository.UpdateWithJsonAsync<AttachmentPatchRequest>(attachment, request);
        return attachment.ToQueryResult();
    }
}