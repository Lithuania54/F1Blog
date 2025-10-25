using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace F1.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<IndexModel> _logger;

        // CarouselImageUrls: populated from src/F1.Web/wwwroot/images/ expecting 1..10 with common extensions
        public List<string> CarouselImageUrls { get; set; } = new();

        // If no images found, this contains a friendly message for the view.
        public string? CarouselNotFoundMessage { get; private set; }

        public IndexModel(IWebHostEnvironment env, ILogger<IndexModel> logger)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnGet()
        {
            try
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var wwwImagesDir = Path.Combine(webRoot, "images");
                var repoImagesDir = Path.Combine(_env.ContentRootPath, "images");

                // Preferred extensions to check (order matters)
                var preferredExts = new[] { ".png", ".avif", ".webp", ".jpg", ".jpeg" };

                // If neither location exists, bail early with a friendly message
                if (!Directory.Exists(wwwImagesDir) && !Directory.Exists(repoImagesDir))
                {
                    _logger.LogWarning("Carousel images directories not found: {WwwImagesDir} or {RepoImagesDir}. Place 1..10 images into one of these paths.", wwwImagesDir, repoImagesDir);
                    CarouselNotFoundMessage = "No carousel images found. Add 1.png..10.png (or 1.avif..1.webp) to src/F1.Web/wwwroot/images/ or src/F1.Web/images/ to enable the hero carousel.";
                    return;
                }

                // For each index 1..10 pick the first available extension in preferredExts,
                // preferring webroot images over repo-level images.
                for (int i = 1; i <= 10; i++)
                {
                    foreach (var ext in preferredExts)
                    {
                        var fileName = $"{i}{ext}";

                        // check webroot (static) first
                        if (Directory.Exists(wwwImagesDir))
                        {
                            var physicalWww = Path.Combine(wwwImagesDir, fileName);
                            if (System.IO.File.Exists(physicalWww))
                            {
                                CarouselImageUrls.Add($"/images/{fileName}");
                                break; // found for this index, continue to next index
                            }
                        }

                        // check repo-level images folder next
                        if (Directory.Exists(repoImagesDir))
                        {
                            var physicalRepo = Path.Combine(repoImagesDir, fileName);
                            if (System.IO.File.Exists(physicalRepo))
                            {
                                // NOTE: serving repo images at /images requires static file mapping in Program.cs (optional)
                                CarouselImageUrls.Add($"/images/{fileName}");
                                break; // found for this index
                            }
                        }
                    }
                }

                if (!CarouselImageUrls.Any())
                {
                    CarouselNotFoundMessage = "No carousel images found. Add files named 1.png..10.png (or 1.avif..1.webp) to src/F1.Web/wwwroot/images/ or src/F1.Web/images/.";
                    _logger.LogInformation("No carousel images (1..10) were found under {WwwImagesDir} or {RepoImagesDir}", wwwImagesDir, repoImagesDir);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while scanning carousel images");
                CarouselNotFoundMessage = "Unable to load hero images at the moment.";
            }

            // TODO: For production, store canonical images in a CDN or managed assets bucket and reference them here.
        }
    }
}
