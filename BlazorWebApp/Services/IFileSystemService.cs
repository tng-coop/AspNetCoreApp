using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services;

public interface IFileSystemService
{
    Task<List<FileSystemNode>> GetRootAsync();
    Task<FileSystemNode?> GetNodeAsync(string path);
}

