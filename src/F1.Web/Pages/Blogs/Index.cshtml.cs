using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using F1.Web.Data;
using F1.Web.Models;

namespace F1.Web.Pages.Blogs
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db) => _db = db;

        public List<Post> Posts { get; set; } = new();

        public async Task OnGetAsync()
        {
            Posts = await _db.Posts
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
