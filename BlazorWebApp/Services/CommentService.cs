using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorWebApp.Data;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services
{
    public class CommentService : ICommentService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory;
        public CommentService(IDbContextFactory<ApplicationDbContext> factory)
            => _factory = factory;

        private ApplicationDbContext CreateDb() => _factory.CreateDbContext();

        public async Task AddAsync(string text)
        {
            await using var db = CreateDb();
            var entity = new Comment
            {
                Id = Guid.NewGuid(),
                Text = text,
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Comments.Add(entity);
            await db.SaveChangesAsync();
        }

        public async Task<List<CommentDto>> ListAsync()
        {
            await using var db = CreateDb();
            return await db.Comments
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }
    }
}
