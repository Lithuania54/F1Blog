using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using F1.Web.Data;
using F1.Web.Models;

namespace F1.Web.Pages.Blogs
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public DetailsModel(ApplicationDbContext db) => _db = db;

        public Post? Post { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Post = await _db.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (Post == null) return NotFound();
            return Page();
        }
    }
}
