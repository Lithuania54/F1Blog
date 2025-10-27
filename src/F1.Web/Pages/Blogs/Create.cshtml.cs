using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using F1.Web.Data;
using F1.Web.Models;
using Microsoft.AspNetCore.Authorization;

namespace F1.Web.Pages.Blogs
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ApplicationDbContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Post Post { get; set; } = new();

        public void OnGet()
        {
            // nothing special
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Title is bound via Post.Title; ensure it's present
            if (string.IsNullOrWhiteSpace(Post?.Title))
                ModelState.AddModelError("Post.Title", "Title is required.");

            // Content is collected from multiple blocks in the form
            if (string.IsNullOrWhiteSpace(Request.Form["ContentBlocks"]))
                ModelState.AddModelError("Post.Content", "Content is required.");

            // Require uploader name
            if (string.IsNullOrWhiteSpace(Post?.AuthorName))
                ModelState.AddModelError("Post.AuthorName", "Your name is required.");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var contentBlocks = Request.Form["ContentBlocks"].Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                var content = string.Join("\n\n", contentBlocks);

                var hashtags = Request.Form["HashtagInputs"]
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .ToArray();
                var hashtagsCsv = string.Join(',', hashtags);

                Post.Content = content;
                Post.Hashtags = hashtagsCsv;
                Post.CreatedAt = DateTime.UtcNow;
                Post.CreatedByUserName = User?.Identity?.Name ?? string.Empty;

                _context.Posts.Add(Post);
                await _context.SaveChangesAsync();

                return RedirectToPage("/Blogs/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog post");
                ModelState.AddModelError(string.Empty, "Unable to save the post at the moment.");
                return Page();
            }
        }
    }
}
