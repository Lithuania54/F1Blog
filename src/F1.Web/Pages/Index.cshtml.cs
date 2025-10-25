// Index page: detects carousel images from either `wwwroot/images/` or
// `ContentRootPath/images/` (repo-level). The CarouselImageUrls property is
// produced server-side and consumed by the carousel JS via the data-images
// attribute. To move images to production, copy files into
// `src/F1.Web/wwwroot/images/` so they are served by the default static
// file provider.
// ASSUMPTION: images named 1.avif..10.avif (or common image extensions).
using System.Text.Json;
using F1.Web.Models;
using F1.Web.Services;

namespace F1.Web.Pages;

public class IndexModel : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
{
    private readonly MarkdownService _md;
    private readonly IWebHostEnvironment _env;

    public List<RenderedPost> Posts { get; set; } = new();
    public List<Driver> Drivers { get; set; } = new();
    public List<RaceItem> Races { get; set; } = new();
    public List<CaseStudy> Works { get; set; } = new();
    public RaceItem? NextRace => Races.OrderBy(r => r.Date).FirstOrDefault();
    // CarouselImageUrls: URLs that will be consumed by the client carousel.
    // ASSUMPTION: images are named 1.avif..10.avif (or PNG/JPG/WebP). We prefer
    // `wwwroot/images/` so the browser can fetch `/images/...`. If missing there,
    // files under the repository-level `src/F1.Web/images/` (ContentRootPath/images)
    // are also served via the static file mapping in Program.cs (development convenience).
    public List<string> CarouselImageUrls { get; set; } = new();

    public IndexModel(MarkdownService md, IWebHostEnvironment env)
    {
        _md = md;
        _env = env;
    }

    public void OnGet()
    {
        Posts = _md.GetAllPosts().ToList();

        var dataFile = Path.Combine(_env.ContentRootPath, "Data", "sample-data.json");
        if (System.IO.File.Exists(dataFile))
        {
            var doc = JsonSerializer.Deserialize<SampleData>(System.IO.File.ReadAllText(dataFile))!;
            Drivers = doc.Drivers;
            Races = doc.Races.OrderBy(r => r.Date).ToList();
            Works = doc.Works;
        }
        else
        {
            // fallback: empty lists
        }

        // Determine available carousel images. Check common extensions and prefer
        // wwwroot/images first so the browser can request /images/<file>. If not
        // found there, look under ContentRootPath/images which we also expose at
        // the /images request path via Program.cs static file mapping (dev usage).
        try
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var wwwImgDir = Path.Combine(webRoot, "images");
            var repoImgDir = Path.Combine(_env.ContentRootPath, "images");
            // Only allow PNG images named 1.png .. 10.png per requirement.
            for (int i = 1; i <= 10; i++)
            {
                var fileName = i + ".png";
                var p1 = Path.Combine(wwwImgDir, fileName);
                var p2 = Path.Combine(repoImgDir, fileName);
                if (System.IO.File.Exists(p1))
                {
                    CarouselImageUrls.Add($"/images/{fileName}");
                }
                else if (System.IO.File.Exists(p2))
                {
                    // files under ContentRootPath/images are served at /images because
                    // Program.cs adds a StaticFileOptions mapping for that path. This
                    // lets us avoid copying files during development.
                    CarouselImageUrls.Add($"/images/{fileName}");
                }
            }
        }
        catch
        {
            // ignore and allow fallback
        }

        // Fallback: if no local images, provide unsplash placeholders
        if (!CarouselImageUrls.Any())
        {
            CarouselImageUrls = new List<string>
            {
                "https://images.unsplash.com/photo-1503376780353-7e6692767b70?q=80&w=1200&auto=format&fit=crop",
                "https://images.unsplash.com/photo-1549921296-3a7d9f4a4d4c?q=80&w=1200&auto=format&fit=crop",
                "https://images.unsplash.com/photo-1525609004556-c46c7d6cf023?q=80&w=1200&auto=format&fit=crop"
            };
        }
    }
}
