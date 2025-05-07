using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using BlazorWebApp.Data;
using System;
using System.IO;
using System.Threading.Tasks;

[ApiController]
[Route("api/images")]
public class ImagesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment  _env;

    public ImagesController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db  = db;
        _env = env;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var asset = new ImageAsset {
            Id          = Guid.NewGuid(),
            Content     = ms.ToArray(),
            ContentType = file.ContentType,
            UploadedAt  = DateTimeOffset.UtcNow
        };
        _db.Images.Add(asset);
        await _db.SaveChangesAsync();

        var url = Url.Action(nameof(Get), "Images", new { id = asset.Id }, Request.Scheme);
        return Ok(new { url });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var img = await _db.Images.FindAsync(id);
        if (img == null)
            return NotFound();

        var cacheDir  = Path.Combine(_env.WebRootPath, "imgcache");
        var fileName  = $"{id}.bin";
        var cachePath = Path.Combine(cacheDir, fileName);

        if (!System.IO.File.Exists(cachePath))
        {
            Directory.CreateDirectory(cacheDir);
            await System.IO.File.WriteAllBytesAsync(cachePath, img.Content);
        }

        return PhysicalFile(cachePath, img.ContentType);
    }
}
