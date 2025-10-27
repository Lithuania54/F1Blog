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
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Blogs/Index");
        }
    }
}
