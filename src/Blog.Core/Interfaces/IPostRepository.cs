using System.Collections.Generic;
using System.Threading.Tasks;
using Blog.Core.Models;

namespace Blog.Core.Interfaces
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetPublishedAsync(int page = 1, int pageSize = 10);
        Task<Post?> GetBySlugAsync(string slug);
        Task<IEnumerable<Post>> SearchAsync(string query, int page = 1, int pageSize = 20);
        Task AddAsync(Post post);
        Task UpdateAsync(Post post);
        Task DeleteAsync(Post post);
    }
}
