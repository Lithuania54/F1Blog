using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Core.Interfaces;
using Blog.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure
{
    public class EfPostRepository : IPostRepository
    {
        private readonly BlogDbContext _db;
        public EfPostRepository(BlogDbContext db) => _db = db;

        public async Task AddAsync(Post post)
        {
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Post post)
        {
            _db.Posts.Remove(post);
            await _db.SaveChangesAsync();
        }

        public async Task<Post?> GetBySlugAsync(string slug)
        {
            return await _db.Posts
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.Category)
                .Include(p => p.AuthorProfile)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<IEnumerable<Post>> GetPublishedAsync(int page = 1, int pageSize = 10)
        {
            return await _db.Posts
                .Where(p => p.IsPublished && p.PublishedAt <= System.DateTime.UtcNow)
                .OrderByDescending(p => p.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> SearchAsync(string query, int page = 1, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<Post>();
            // Simple SQL LIKE search across Title and Content
            var q = query.Trim();
            return await _db.Posts
                .Where(p => EF.Functions.Like(p.Title, $"%{q}%") || EF.Functions.Like(p.Content, $"%{q}%"))
                .OrderByDescending(p => p.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task UpdateAsync(Post post)
        {
            _db.Posts.Update(post);
            await _db.SaveChangesAsync();
        }
    }
}
