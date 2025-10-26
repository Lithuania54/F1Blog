using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using F1.Web.Data;
using F1.Web.Models;

namespace F1.Web.Pages.Blogs
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Post Post { get; set; } = new Post();

        // Note: ContentBlocks and HashtagInputs bind to request fields in the view
        [BindProperty]
        public List<string>? ContentBlocks { get; set; }

        [BindProperty]
        public List<string>? HashtagInputs { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Normalize inputs
            var nonEmptyBlocks = (ContentBlocks ?? new List<string>())
                                 .Select(b => b?.Trim())
                                 .Where(s => !string.IsNullOrEmpty(s))
                                 .ToList();

            if (!nonEmptyBlocks.Any())
            {
                ModelState.AddModelError(string.Empty, "Please add at least one content block.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Combine blocks into final Content with paragraph spacing
            Post.Content = string.Join("\n\n", nonEmptyBlocks);

            // Build hashtags (take up to first 3 non-empty)
            var hashtags = (HashtagInputs ?? new List<string>())
                           .Select(h => h?.Trim().TrimStart('#'))
                           .Where(h => !string.IsNullOrEmpty(h))
                           .Take(3);
            Post.Hashtags = string.Join(",", hashtags);

            // Author info
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Post.AuthorId = user.Id;
                // store display name so it is preserved even if username changes
                Post.AuthorName = User.Identity?.Name ?? user.Email;
            }

            // Append author name to end of content
            if (!string.IsNullOrEmpty(Post.AuthorName))
            {
                Post.Content += $"\n\n— {Post.AuthorName}";
            }

            Post.CreatedAt = DateTime.UtcNow;

            // Save
            _context.Posts.Add(Post);
            await _context.SaveChangesAsync();

            // Redirect to blogs index or details
            return RedirectToPage("/Blogs/Index");
        }
    }
}
