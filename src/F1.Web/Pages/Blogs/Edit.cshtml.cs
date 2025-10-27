using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using F1.Web.Data;
using F1.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace F1.Web.Pages.Blogs
{
    [Authorize] // user must be logged in to edit
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EditModel> _logger;

        public EditModel(ApplicationDbContext context, ILogger<EditModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Post Post { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            // only author or admin may edit
            var userName = User?.Identity?.Name;
            if (userName != post.AuthorName && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            Post = post;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            // Load existing post
            var existing = await _context.Posts.FindAsync(id);
            if (existing == null) return NotFound();

            var userName = User?.Identity?.Name;
            if (userName != existing.AuthorName && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                // Read updated values from the form (support same form layout as Create)
                var contentBlocks = Request.Form["ContentBlocks"].Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                var content = string.Join("\n\n", contentBlocks);

                var hashtags = Request.Form["HashtagInputs"]
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .ToArray();
                var hashtagsCsv = string.Join(',', hashtags);

                // Update allowed fields
                existing.Title = Request.Form["Post.Title"].FirstOrDefault() ?? existing.Title;
                existing.Content = string.IsNullOrWhiteSpace(content) ? existing.Content : content;
                existing.Hashtags = string.IsNullOrWhiteSpace(hashtagsCsv) ? existing.Hashtags : hashtagsCsv;
                existing.ImageUrl = Request.Form["Post.ImageUrl"].FirstOrDefault() ?? existing.ImageUrl;
                existing.UpdatedAt = DateTime.UtcNow;

                _context.Posts.Update(existing);
                await _context.SaveChangesAsync();

                return RedirectToPage("/Blogs/Details", new { id = existing.Id });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating post id {Id}", id);
                ModelState.AddModelError(string.Empty, "Concurrency problem while updating, please try again.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post id {Id}", id);
                ModelState.AddModelError(string.Empty, "Unable to update the post at the moment.");
                return Page();
            }
        }
    }
}
