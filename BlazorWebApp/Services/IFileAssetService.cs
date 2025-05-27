using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BlazorWebApp.Data;

namespace BlazorWebApp.Services
{
    public interface IFileAssetService
    {
        Task<FileAsset> UploadAsync(IFormFile file);
        Task<FileAsset?> GetAsync(Guid id);
        string GetCachePath(FileAsset asset);
    }
}

