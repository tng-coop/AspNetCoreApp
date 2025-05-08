using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using BlazorWebApp.Data;
using System;
using System.IO;
using System.Threading.Tasks;

[ApiController]
[Route("api/images")]
// Enable client/CDN caching for 1 hour
[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
public class ImagesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly string _cacheDir;

    public ImagesController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        // Prepare the disk cache directory once
        _cacheDir = Path.Combine(env.WebRootPath, "imgcache");
        Directory.CreateDirectory(_cacheDir);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var asset = new ImageAsset
        {
            Id = Guid.NewGuid(),
            Content = ms.ToArray(),
            ContentType = file.ContentType,
            UploadedAt = DateTimeOffset.UtcNow
        };
        _db.Images.Add(asset);
        await _db.SaveChangesAsync();

        var url = Url.Action(nameof(Get), "Images", new { id = asset.Id }, Request.Scheme);
        return Ok(new { location = url });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var img = await _db.Images.FindAsync(id);
        if (img == null)
            return NotFound();

        // Determine extension from content-type
        string extension = img.ContentType switch
        {
            "image/png"  => ".png",
            "image/jpeg" => ".jpg",
            "image/gif"  => ".gif",
            _             => ".bin"
        };

        var fileName = id + extension;
        var cachePath = Path.Combine(_cacheDir, fileName);

        // Write cache file if missing
        if (!System.IO.File.Exists(cachePath))
        {
            await System.IO.File.WriteAllBytesAsync(cachePath, img.Content);
        }

        // Serve the file via optimized file streaming
        return PhysicalFile(cachePath, img.ContentType);
    }
}
