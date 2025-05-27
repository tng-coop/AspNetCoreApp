using Microsoft.AspNetCore.Mvc;
using BlazorWebApp.Services;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Threading.Tasks;

[ApiController]
[Route("api/files")]
// Enable client/CDN caching for 1 hour
[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
public class FilesController : ControllerBase
{
    private readonly IFileAssetService _service;

    public FilesController(IFileAssetService service)
    {
        _service = service;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file.");

        Console.WriteLine($"FilesController.Upload: received {file.FileName} ({file.Length} bytes)");

        var asset = await _service.UploadAsync(file);

        Console.WriteLine($"FilesController.Upload: stored as {asset.Id}");

        var url = Url.Action(nameof(Get), "Files", new { id = asset.Id, fileName = asset.FileName }, Request.Scheme);
        return Ok(new { location = url });
    }

    [HttpGet("{id:guid}/{fileName?}")]
    public async Task<IActionResult> Get(Guid id, string? fileName)
    {
        var fileAsset = await _service.GetAsync(id);
        if (fileAsset == null)
            return NotFound();

        var cachePath = _service.GetCachePath(fileAsset);

        // Serve the file via optimized file streaming
        var contentType = fileAsset.ContentType;
        if (string.IsNullOrWhiteSpace(contentType) || !MediaTypeHeaderValue.TryParse(contentType, out _))
        {
            var ext = Path.GetExtension(fileAsset.FileName);
            contentType = string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase)
                ? "application/pdf"
                : "application/octet-stream";
        }

        var result = PhysicalFile(cachePath, contentType);

        if (contentType == "application/pdf")
        {
            result.EnableRangeProcessing = true;
            Response.Headers["Accept-Ranges"] = "bytes";
            var info = new FileInfo(cachePath);
            var hashInput = System.Text.Encoding.UTF8.GetBytes(info.LastWriteTimeUtc.Ticks + ":" + info.Length);
            var hash = System.Security.Cryptography.MD5.HashData(hashInput);
            var etag = "\"" + Convert.ToHexString(hash).ToLowerInvariant().Substring(0, 16) + "\"";
            Response.Headers["ETag"] = etag;
            Response.Headers["Last-Modified"] = info.LastWriteTimeUtc.ToString("R");
        }

        return result;
    }
}
