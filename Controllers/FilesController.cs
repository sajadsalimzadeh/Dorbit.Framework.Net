using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Attachments;
using Dorbit.Framework.Contracts.Files;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Entities;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Repositories;
using Dorbit.Framework.Services;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Dorbit.Framework.Controllers;

[ApiExplorerSettings(GroupName = "framework")]
[Route("Framework/[controller]")]
public class FilesController(FileService fileService) : BaseController
{
    private static readonly HashSet<string> InlineExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp", ".pdf", ".txt", ".csv"
    };

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

    [HttpPost, Auth, AntiDos(AntiDosAttribute.DurationType.Hour, 20)]
    public Task<QueryResult<string>> UploadAsync([FromForm] AttachmentUploadPrivateRequest request)
    {
        request.UserId = GetUserId();
        var file = Request.Form.Files[0];
        request.Name = file.FileName;
        request.Stream = file.OpenReadStream();
        return fileService.UploadAsync(request).ToQueryResultAsync();
    }

    [HttpPost("Bulk"), Auth("File-Bulk"), AntiDos(AntiDosAttribute.DurationType.Minute, 1)]
    public async Task<CommandResult> UploadAsync([FromForm] IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        await using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        foreach (var entry in archive.Entries)
        {
            await using var entryStream = await entry.OpenAsync();
            using var reader = new StreamReader(stream);
            await fileService.UploadAsync(new AttachmentUploadPrivateRequest()
            {
                UserId = GetUserId(),
                Name = entry.Name,
                Stream = entryStream,
            });
        }

        return Succeed();
    }

    [HttpGet("{filename}"), Auth(IsOptional = true)]
    public async Task<IActionResult> GetAsync([FromRoute] string filename)
    {
        var fileDto = await fileService.GetFileAsync(filename);
        return FileResult(fileDto, filename, download: false);
    }

    [HttpGet("{filename}/Download"), Auth(IsOptional = true)]
    public async Task<IActionResult> DownloadAsync([FromRoute] string filename)
    {
        var fileDto = await fileService.GetFileAsync(filename);
        return FileResult(fileDto, filename, download: true);
    }

    [HttpGet("{filename}/Info"), Auth]
    public Task<QueryResult<Attachment>> GetInfoAsync([FromRoute] string filename)
    {
        return fileService.GetInfoAsync(filename).ToQueryResultAsync();
    }

    [HttpPatch("{filename}/Info"), Auth]
    public Task<QueryResult<Attachment>> PatchInfoAsync([FromRoute] string filename, [FromBody] JsonElement request)
    {
        return fileService.PatchInfoAsync(filename, request).ToQueryResultAsync();
    }
}