using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services
{
    public interface ICommentService
    {
        Task AddAsync(string text);
        Task<List<CommentDto>> ListAsync();
    }
}
