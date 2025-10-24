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
    public List<string> Images { get; set; } = new();

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

        // Determine available carousel images in wwwroot/images named 1.png..10.png
        try
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var imgDir = Path.Combine(webRoot, "images");
            for (int i = 1; i <= 10; i++)
            {
                var name = i + ".png";
                var p = Path.Combine(imgDir, name);
                if (System.IO.File.Exists(p))
                {
                    Images.Add($"/images/{name}");
                }
            }
        }
        catch
        {
            // ignore
        }

        // Fallback: if no local images, provide unsplash placeholders
        if (!Images.Any())
        {
            Images = new List<string>
            {
                "https://images.unsplash.com/photo-1503376780353-7e6692767b70?q=80&w=1200&auto=format&fit=crop",
                "https://images.unsplash.com/photo-1549921296-3a7d9f4a4d4c?q=80&w=1200&auto=format&fit=crop",
                "https://images.unsplash.com/photo-1525609004556-c46c7d6cf023?q=80&w=1200&auto=format&fit=crop"
            };
        }
    }
}
