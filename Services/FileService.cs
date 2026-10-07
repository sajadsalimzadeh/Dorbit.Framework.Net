using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Attachments;
using Dorbit.Framework.Contracts.Files;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Repositories;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class FileService(
    IMemoryCache memoryCache,
    IIdentityService identityService,
    IOptions<ConfigFile> configFileOptions,
    IHttpContextAccessor httpContextAccessor,
    AttachmentRepository attachmentRepository)
{
    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".htm", ".svg", ".js", ".mjs", ".xml", ".xhtml", ".shtml",
        ".aspx", ".asp", ".php", ".exe", ".dll", ".bat", ".cmd", ".ps1",
        ".hta", ".vbs", ".wsf", ".scr", ".msi", ".com", ".jar", ".cshtml"
    };

    private string GetFilePath(string filename, Attachment attachment)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new OperationException(FrameworkErrors.FilePathIsInvalid);

        var safeName = Path.GetFileName(filename);
        if (!string.Equals(safeName, filename, StringComparison.Ordinal) ||
            safeName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new OperationException(FrameworkErrors.FilePathIsInvalid);

        var configFile = configFileOptions.Value;
        var basePath = Path.GetFullPath(configFile.BasePath);
        if (attachment is not null && attachment.Drive.IsNotNullOrEmpty() && configFile.Drives is not null)
        {
            if (configFile.Drives.TryGetValue(attachment.Drive, out var drive))
            {
                basePath = Path.Combine(basePath, attachment.Drive);
            }
        }
        Directory.CreateDirectory(basePath);
        var filePath = Path.GetFullPath(Path.Combine(basePath, safeName));
        var relative = Path.GetRelativePath(basePath, filePath);
        if (relative.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(relative))
            throw new OperationException(FrameworkErrors.FilePathIsInvalid);

        return filePath;
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
            httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("FileAuthorization", out var accessToken) == true &&
            attachment.AccessTokens.Contains(accessToken.ToString()))
            return true;

        return false;
    }

    public async Task<FileDto> GetFileAsync(string filename)
    {
        var key = "File-" + filename;
        var fileDto = await memoryCache.GetValueWithLockAsync(key, async () =>
        {
            var dto = new FileDto();
            dto.Attachment = await attachmentRepository.FirstOrDefaultAsync(x => x.Filename == filename);
            var filePath = GetFilePath(filename, dto.Attachment);
            var fileInfo = new FileInfo(filePath);
            if (!File.Exists(filePath)) throw new FileNotFoundException();
            dto.LastModifyTime = fileInfo.LastWriteTime;
            dto.Content = await File.ReadAllBytesAsync(filePath);
            return dto;
        }, TimeSpan.FromMinutes(5));
        if (fileDto.Attachment != null && !ValidateAccess(fileDto.Attachment))
            throw new UnauthorizedAccessException();

        return fileDto;
    }

    public async Task<string> UploadAsync([FromForm] AttachmentUploadRequest request)
    {
        var size = request.Stream.Length;
        if (size > configFileOptions.Value.MaxSize)
        {
            var hasAccess = false;
            if (configFileOptions.Value.MaxSizeAccessibility is not null)
            {
                foreach (var access in configFileOptions.Value.MaxSizeAccessibility)
                {
                    if (identityService.Identity.HasAccess(access.Key) && size < access.Value)
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

        var ext = Path.GetExtension(request.Name);
        if (BlockedExtensions.Contains(ext))
            throw new OperationException(FrameworkErrors.FileTypeIsNotAllowed);

        var filename = Guid.NewGuid() + ext;
        var attachment = new Attachment()
        {
            UserId = request.UserId,
            Drive = request.Drive,
            Filename = filename,
            Size = size,
            Access = request.Access,
            IsPrivate = request.IsPrivate,
        };
        
        if (configFileOptions.Value.Drives is not null && configFileOptions.Value.Drives.TryGetValue(request.Drive, out var drive))
        {
            attachment.Access = drive.Access;
        }
        
        var filePath = GetFilePath(filename, attachment);
        using var ms = new MemoryStream();
        await request.Stream.CopyToAsync(ms);
        await File.WriteAllBytesAsync(filePath, ms.ToArray());
        await attachmentRepository.InsertAsync(attachment);

        return filename;
    }
}