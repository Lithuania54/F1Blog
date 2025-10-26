using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using F1.Web.Data;
using F1.Web.Models;

namespace F1.Web.Pages.Blogs
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<CreateModel> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [BindProperty]
        public Post Post { get; set; } = new Post();

        // Bound to repeated textarea inputs named "ContentBlocks"
        [BindProperty]
        public List<string>? ContentBlocks { get; set; }

        // Bound to the three hashtag inputs named "HashtagInputs"
        [BindProperty]
        public List<string>? HashtagInputs { get; set; }

        public void OnGet()
        {
            // no-op
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Log ModelState errors if present
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Create OnPost: ModelState invalid. Errors:");
                foreach (var kv in ModelState)
                {
                    foreach (var err in kv.Value.Errors)
                    {
                        _logger.LogWarning(" - {Key}: {Error}", kv.Key, err.ErrorMessage);
                    }
                }
                return Page();
            }

            // Collect content blocks and validate
            var nonEmptyBlocks = (ContentBlocks ?? new List<string>())
                                 .Select(b => b?.Trim())
                                 .Where(s => !string.IsNullOrEmpty(s))
                                 .ToList();

            if (!nonEmptyBlocks.Any())
            {
                ModelState.AddModelError(string.Empty, "Please add at least one content block.");
                _logger.LogWarning("Create OnPost: no content blocks provided.");
                return Page();
            }

            // Combine blocks into final content with paragraph spacing
            Post.Content = string.Join("\n\n", nonEmptyBlocks);

            // Build hashtags (take up to first 3 non-empty, normalize)
            var tags = (HashtagInputs ?? new List<string>())
                       .Select(h => h?.Trim().TrimStart('#'))
                       .Where(h => !string.IsNullOrEmpty(h))
                       .Take(3);
            Post.Hashtags = string.Join(",", tags);

            // Author info
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Post.AuthorId = user.Id;
                Post.AuthorName = User.Identity?.Name ?? user.Email;
            }

            // Append author name to end of content (optional)
            if (!string.IsNullOrEmpty(Post.AuthorName))
            {
                Post.Content += $"\n\n— {Post.AuthorName}";
            }

            Post.CreatedAt = DateTime.UtcNow;

            try
            {
                _context.Posts.Add(Post);
                var saved = await _context.SaveChangesAsync();
                _logger.LogInformation("Create OnPost: SaveChanges returned {Saved}. New Post.Id = {Id}", saved, Post.Id);

                // Redirect to Details (if exists) or Index otherwise
                return RedirectToPage("/Blogs/Details", new { id = Post.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create OnPost: Exception while saving Post (Title: {Title})", Post?.Title);
                ModelState.AddModelError(string.Empty, "Unable to save the post. Check logs for details.");
                return Page();
            }
        }
    }
}
