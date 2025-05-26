using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services
{
    public interface ICommentService
    {
        event Action<int>? OnUnreadCountChanged;

        Task AddAsync(string text);
        Task<List<CommentDto>> ListAsync();
        Task SetReadStatusAsync(Guid id, bool isRead);
        Task<int> CountUnreadAsync();
    }
}
