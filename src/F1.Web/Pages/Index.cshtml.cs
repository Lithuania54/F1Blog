using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using F1.Web.Models;
using F1.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace F1.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<IndexModel> _logger;

        public List<string> CarouselImageUrls { get; set; } = new();
        public string? CarouselNotFoundMessage { get; private set; }

        public IndexModel(IWebHostEnvironment env, ILogger<IndexModel> logger)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            await LoadCarouselAsync();
        }

        private Task LoadCarouselAsync()
        {
            try
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var wwwImagesDir = Path.Combine(webRoot, "images");
                var repoImagesDir = Path.Combine(_env.ContentRootPath, "images");

                if (!Directory.Exists(wwwImagesDir) && !Directory.Exists(repoImagesDir))
                {
                    _logger.LogWarning("Carousel images directories not found: {WwwImagesDir} or {RepoImagesDir}.", wwwImagesDir, repoImagesDir);
                    CarouselNotFoundMessage = "No carousel images found. Add 1.png..10.png (or 1.avif..1.webp) to src/F1.Web/wwwroot/images/ or src/F1.Web/images/ to enable the hero carousel.";
                    return Task.CompletedTask;
                }

                var preferredExts = new[] { ".png", ".avif", ".webp", ".jpg", ".jpeg" };

                for (int i = 1; i <= 10; i++)
                {
                    foreach (var ext in preferredExts)
                    {
                        var fileName = $"{i}{ext}";

                        if (Directory.Exists(wwwImagesDir))
                        {
                            var physicalWww = Path.Combine(wwwImagesDir, fileName);
                            if (System.IO.File.Exists(physicalWww))
                            {
                                CarouselImageUrls.Add($"/images/{fileName}");
                                break;
                            }
                        }

                        if (Directory.Exists(repoImagesDir))
                        {
                            var physicalRepo = Path.Combine(repoImagesDir, fileName);
                            if (System.IO.File.Exists(physicalRepo))
                            {
                                CarouselImageUrls.Add($"/images/{fileName}");
                                break;
                            }
                        }
                    }
                }

                if (CarouselImageUrls.Count == 0)
                {
                    CarouselNotFoundMessage = "No carousel images found. Add files named 1.png..10.png (or 1.avif..1.webp) to src/F1.Web/wwwroot/images/ or src/F1.Web/images/.";
                    _logger.LogInformation("No carousel images (1..10) were found under {WwwImagesDir} or {RepoImagesDir}.", wwwImagesDir, repoImagesDir);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while scanning carousel images");
                CarouselNotFoundMessage = "Unable to load hero images at the moment.";
            }

            return Task.CompletedTask;
        }
    }
}
