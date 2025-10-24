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
    }
}
