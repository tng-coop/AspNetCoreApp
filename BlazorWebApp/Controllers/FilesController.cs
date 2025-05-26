using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using BlazorWebApp.Data;
using System;
using System.IO;
using System.Threading.Tasks;

[ApiController]
[Route("api/files")]
// Enable client/CDN caching for 1 hour
[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
public class FilesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly string _cacheDir;

    public FilesController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        // Prepare the disk cache directory once
        _cacheDir = Path.Combine(env.WebRootPath, "filecache");
        Directory.CreateDirectory(_cacheDir);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var asset = new FileAsset
        {
            Id = Guid.NewGuid(),
            Content = ms.ToArray(),
            ContentType = file.ContentType,
            FileName = file.FileName,
            UploadedAt = DateTimeOffset.UtcNow
        };
        _db.Files.Add(asset);
        await _db.SaveChangesAsync();

        var url = Url.Action(nameof(Get), "Files", new { id = asset.Id, fileName = asset.FileName }, Request.Scheme);
        return Ok(new { location = url });
    }

    [HttpGet("{id:guid}/{fileName?}")]
    public async Task<IActionResult> Get(Guid id, string? fileName)
    {
        var fileAsset = await _db.Files.FindAsync(id);
        if (fileAsset == null)
            return NotFound();

        var extension = Path.GetExtension(fileAsset.FileName);
        if (string.IsNullOrEmpty(extension))
            extension = ".bin";

        var cacheFileName = id + extension;
        var cachePath = Path.Combine(_cacheDir, cacheFileName);

        // Write cache file if missing
        if (!System.IO.File.Exists(cachePath))
        {
            await System.IO.File.WriteAllBytesAsync(cachePath, fileAsset.Content);
        }

        // Serve the file via optimized file streaming
        var result = PhysicalFile(cachePath, fileAsset.ContentType);

        if (fileAsset.ContentType == "application/pdf")
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
