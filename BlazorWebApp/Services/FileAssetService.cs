using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;

namespace BlazorWebApp.Services
{
    public class FileAssetService : IFileAssetService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory;
        private readonly string _cacheDir;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public FileAssetService(IDbContextFactory<ApplicationDbContext> factory, IWebHostEnvironment env)
        {
            _factory = factory;
            _cacheDir = Path.Combine(env.WebRootPath, "filecache");
            Directory.CreateDirectory(_cacheDir);
        }

        private ApplicationDbContext CreateDb() => _factory.CreateDbContext();

        public async Task<FileAsset> UploadAsync(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var contentType = file.ContentType;
            if (string.IsNullOrWhiteSpace(contentType) || !MediaTypeHeaderValue.TryParse(contentType, out _))
            {
                var ext = Path.GetExtension(file.FileName);
                contentType = string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase)
                    ? "application/pdf"
                    : "application/octet-stream";
            }

            var asset = new FileAsset
            {
                Id = Guid.NewGuid(),
                Content = ms.ToArray(),
                ContentType = contentType,
                FileName = file.FileName,
                UploadedAt = DateTimeOffset.UtcNow
            };

            await using var db = CreateDb();
            db.Files.Add(asset);
            await db.SaveChangesAsync();

            var cachePath = GetCachePath(asset);
            var sem = _locks.GetOrAdd(cachePath, _ => new SemaphoreSlim(1, 1));
            await sem.WaitAsync();
            try
            {
                await System.IO.File.WriteAllBytesAsync(cachePath, asset.Content);
            }
            finally
            {
                sem.Release();
            }

            return asset;
        }

        public async Task<FileAsset?> GetAsync(Guid id)
        {
            await using var db = CreateDb();
            var asset = await db.Files.FindAsync(id);
            if (asset == null)
                return null;

            var cachePath = GetCachePath(asset);
            var sem = _locks.GetOrAdd(cachePath, _ => new SemaphoreSlim(1, 1));
            await sem.WaitAsync();
            try
            {
                if (!System.IO.File.Exists(cachePath))
                {
                    await System.IO.File.WriteAllBytesAsync(cachePath, asset.Content);
                }
            }
            finally
            {
                sem.Release();
            }

            return asset;
        }

        public string GetCachePath(FileAsset asset)
        {
            var extension = Path.GetExtension(asset.FileName);
            if (string.IsNullOrEmpty(extension))
                extension = ".bin";
            return Path.Combine(_cacheDir, asset.Id + extension);
        }
    }
}
