using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Attachments;
using Dorbit.Framework.Contracts.Identities;
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
    private string GetFilePath(string filename)
    {
        var configFile = configFileOptions.Value;
        if (!Directory.Exists(configFile.BasePath)) Directory.CreateDirectory(configFile.BasePath);
        return Path.Combine(configFile.BasePath, filename);
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

        if (attachment.Access.IsNullOrEmpty())
            return true;

        var identity = identityService.Identity;
        if (identity is null)
            return false;

        if (identity.HasAccess(attachment.Access))
            return true;

        if (attachment.UserIds != null && attachment.UserIds.Contains(identity.User.GetId()))
            return true;

        if (attachment.AccessTokens != null)
        {
            if (Request.Headers.TryGetValue("FileAuthorization", out var accessToken) && attachment.AccessTokens.Contains(accessToken))
                return true;
        }

        return false;
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
        memoryCache.Set(key, fileDto, TimeSpan.FromMinutes(5));
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
        var filename = Guid.NewGuid() + ext;
        var filePath = GetFilePath(filename);
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());

        if (request is not null)
        {
            await attachmentRepository.InsertAsync(new Attachment()
            {
                UserId = GetUserId(),
                IsPrivate = true,
                Filename = filename,
                Size = file.Length,
                Access = request.Access,
            });
        }

        return new QueryResult<string>(filename);
    }

    [HttpGet("{filename}")]
    [ResponseCache(Duration = 365 * 24 * 60 * 60)]
    public async Task<IActionResult> GetAsync([FromRoute] string filename)
    {
        var fileDto = await GetFileAsync(filename);
        return File(fileDto.Content, MimeTypeUtil.GetMimeTypeByFilename(filename));
    }

    [HttpGet("{filename}/Download"), Auth(IsOptional = true)]
    public async Task<IActionResult> DownloadAsync([FromRoute] string filename)
    {
        var fileDto = await GetFileAsync(filename);
        return File(fileDto.Content, "application/octet-stream", filename);
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
    public async Task<QueryResult<Attachment>> PatchInfoAsync([FromRoute] string filename, [FromBody] AttachmentPatchRequest request)
    {
        var attachment = await attachmentRepository.FirstOrDefaultAsync(x => x.Filename == filename)
                         ?? throw new OperationException(FrameworkErrors.EntityNotFound);

        if (identityService.Identity.User.GetId() != attachment.UserId)
            throw new UnauthorizedAccessException();

        attachment = await attachmentRepository.PatchAsync(attachment, request);
        return attachment.ToQueryResult();
    }
}