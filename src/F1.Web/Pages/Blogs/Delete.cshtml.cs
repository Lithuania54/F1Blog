using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using F1.Web.Data;

namespace F1.Web.Pages.Blogs
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public DeleteModel(ApplicationDbContext context) => _context = context;

        public F1.Web.Models.Post? Post { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Post = await _context.Posts.FindAsync(id);
            if (Post == null) return NotFound();
            var userName = User?.Identity?.Name ?? string.Empty;
            var isAdmin = User?.IsInRole("Admin") ?? false;
            var isOwner = !string.IsNullOrWhiteSpace(Post.CreatedByUserName) && string.Equals(Post.CreatedByUserName, userName, StringComparison.Ordinal);
            var hasNoOwner = string.IsNullOrWhiteSpace(Post.CreatedByUserName);
            if (!(isOwner || isAdmin || hasNoOwner)) return Forbid();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var userName = User?.Identity?.Name ?? string.Empty;
            var isAdmin = User?.IsInRole("Admin") ?? false;
            var isOwner = !string.IsNullOrWhiteSpace(post.CreatedByUserName) && string.Equals(post.CreatedByUserName, userName, StringComparison.Ordinal);
            var hasNoOwner = string.IsNullOrWhiteSpace(post.CreatedByUserName);
            if (!(isOwner || isAdmin || hasNoOwner)) return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Blogs/Index");
        }
    }
}
