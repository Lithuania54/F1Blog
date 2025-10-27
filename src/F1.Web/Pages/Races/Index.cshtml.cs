using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using F1.Web.Models;

namespace F1.Web.Pages.Races;

public class IndexModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public List<RaceItem> Races { get; set; } = new();
    public List<string> TrackImageUrls { get; set; } = new();

    public IndexModel(IWebHostEnvironment env)
    {
        _env = env;
    }

    public void OnGet()
    {
        var dataFile = Path.Combine(Directory.GetCurrentDirectory(), "src", "F1.Web", "Data", "races-2025.json");
        if (System.IO.File.Exists(dataFile))
        {
            var doc = JsonSerializer.Deserialize<JsonElement>(System.IO.File.ReadAllText(dataFile));
            if (doc.TryGetProperty("Races", out var r))
            {
                Races = JsonSerializer.Deserialize<List<RaceItem>>(r.GetRawText())!.OrderBy(x => x.Date).ToList();
            }
        }

    var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
    var wwwImgDir = Path.Combine(webRoot, "images2");
    var repoImgDir = Path.Combine(_env.ContentRootPath, "images2");

        var names = new List<string>();
        if (Directory.Exists(wwwImgDir))
        {
            var files = Directory.GetFiles(wwwImgDir)
                .Select(Path.GetFileName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n!)
                .ToArray();
            names.AddRange(files);
        }
        if (Directory.Exists(repoImgDir))
        {
            var files = Directory.GetFiles(repoImgDir)
                .Select(Path.GetFileName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n!)
                .ToArray();
            names.AddRange(files);
        }

        for (int i = 1; i <= 24; i++)
        {
            var fileName = i + ".png";
            var p1 = Path.Combine(wwwImgDir, fileName);
            var p2 = Path.Combine(repoImgDir, fileName);
            if (System.IO.File.Exists(p1) || System.IO.File.Exists(p2))
            {
                TrackImageUrls.Add($"/images2/{fileName}");
            }
        }
    }
}
