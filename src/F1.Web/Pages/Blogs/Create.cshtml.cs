using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YourProjectNamespace.Data; // Replace with your actual namespace
using YourProjectNamespace.Models; // Replace with your actual namespace
using System.Threading.Tasks;

namespace YourProjectNamespace.Pages.Blog
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Post Post { get; set; }

        public void OnGet()
        {
            // Any initialization if needed
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Posts.Add(Post);
            await _context.SaveChangesAsync();

            // Redirect back to the index page after successful creation
            return RedirectToPage("./Index");
        }
    }
}
