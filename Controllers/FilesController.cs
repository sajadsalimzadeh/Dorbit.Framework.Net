using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Dorbit.Framework.Controllers;

[ApiExplorerSettings(GroupName = "framework")]
[Route("Framework/[controller]")]
public class FilesController(FileService fileService, AttachmentRepository attachmentRepository) : BaseController
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

    [HttpGet, Auth("File-View")]
    public Task<QueryResult<List<AttachmentFullDto>>> GetAllFileAsync()
    {
        return attachmentRepository.Set().ToListAsync().MapToAsync<Attachment, AttachmentFullDto>().ToQueryResultAsync();
    }

    [HttpGet("{id:guid}"), Auth("File-View")]
    public Task<QueryResult<AttachmentFullDto>> GetInfoAsync([FromRoute] Guid id)
    {
        return attachmentRepository.GetByIdAsync(id).MapToAsync<Attachment, AttachmentFullDto>().ToQueryResultAsync();
    }

    [HttpPost, Auth, AntiDos(AntiDosAttribute.DurationType.Minute, 3)]
    public Task<QueryResult<string>> UploadAsync([FromForm] AttachmentUploadRequest request)
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
            await fileService.UploadAsync(new AttachmentUploadRequest()
            {
                UserId = GetUserId(),
                Name = entry.Name,
                Stream = entryStream,
            });
        }

        return Succeed();
    }

    [HttpPatch("{id:guid}"), Auth("File-Save")]
    public async Task<QueryResult<AttachmentFullDto>> PatchAsync([FromRoute] string filename, [FromBody] AttachmentUploadRequest request)
    {
        var attachment = await attachmentRepository.Set().FirstOrDefaultAsync(x => x.Filename == filename);
        attachment.Drive = request.Drive;
        attachment.Name = request.Name;
        attachment.Description = request.Description;
        attachment.Access = request.Access;
        attachment.IsPrivate = request.IsPrivate;
        if (request.UserIds is not null) attachment.UserIds = request.UserIds;
        if (request.AccessTokens is not null) attachment.AccessTokens = request.AccessTokens;
        return await attachmentRepository.UpdateAsync(attachment).MapToAsync<Attachment, AttachmentFullDto>().ToQueryResultAsync();
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
}