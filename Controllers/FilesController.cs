using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Dorbit.Framework.Controllers;

[ApiExplorerSettings(GroupName = "framework")]
[Route("Framework/[controller]")]
public class FilesController(IOptions<ConfigFile> options, IMemoryCache memoryCache) : BaseController
{
    private readonly ConcurrentDictionary<string, object> _monitors = new();
    private readonly ConfigFile _configFile = options.Value;

    private string GetFilePath(string filename)
    {
        if (!Directory.Exists(_configFile.BasePath)) Directory.CreateDirectory(_configFile.BasePath);
        return Path.Combine(_configFile.BasePath, filename);
    }

    private class FileDto
    {
        public DateTimeOffset? LastModifyTime { get; set; }
        public byte[] Content { get; set; }
    }

    private FileDto GetFile(string filename)
    {
        var monitor = _monitors.GetOrAdd(filename, () => new { });
        lock (monitor)
        {
            if (!memoryCache.TryGetValue(filename, out FileDto dto))
            {
                dto = new FileDto();
                var filePath = GetFilePath(filename);
                var fileInfo = new FileInfo(filePath);
                if (!System.IO.File.Exists(filePath)) throw new FileNotFoundException();
                dto.LastModifyTime = fileInfo.LastWriteTime;
                dto.Content = System.IO.File.ReadAllBytes(filePath);
                memoryCache.Set(filename, dto, TimeSpan.FromHours(1));
            }

            return dto;
        }
    }

    [HttpGet("{filename}")]
    [ResponseCache(Duration = 365 * 24 * 60 * 60)]
    public IActionResult GetAsync([FromRoute] string filename)
    {
        var fileDto = GetFile(filename);
        return File(fileDto.Content, MimeTypeUtil.GetMimeTypeByFilename(filename));
    }

    [HttpGet("{filename}/Download")]
    public IActionResult DownloadAsync([FromRoute] string filename)
    {
        var fileDto = GetFile(filename);
        return File(fileDto.Content, "application/octet-stream");
    }

    [HttpPost, Auth]
    public async Task<QueryResult<string>> UploadAsync()
    {
        var file = Request.Form.Files[0];
        var ext = Path.GetExtension(file.FileName);
        var filename = Guid.CreateVersion7() + ext;
        var filePath = GetFilePath(filename);
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());
        return new QueryResult<string>(filename);
    }
}