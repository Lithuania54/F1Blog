using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using F1.Web.Models;

namespace F1.Web.Pages.Races;

public class IndexModel : PageModel
{
    public List<RaceItem> Races { get; set; } = new();
    public List<string> TrackImageUrls { get; set; } = new();

    public void OnGet()
    {
        // Load races from Data/races-2025.json if present
        var dataFile = Path.Combine(Directory.GetCurrentDirectory(), "src", "F1.Web", "Data", "races-2025.json");
        if (System.IO.File.Exists(dataFile))
        {
            var doc = JsonSerializer.Deserialize<JsonElement>(System.IO.File.ReadAllText(dataFile));
            if (doc.TryGetProperty("Races", out var r))
            {
                Races = JsonSerializer.Deserialize<List<RaceItem>>(r.GetRawText())!.OrderBy(x => x.Date).ToList();
            }
        }

        // Detect track images in wwwroot/images2 or ContentRootPath/images2
        var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "src", "F1.Web", "wwwroot");
        var wwwImgDir = Path.Combine(webRoot, "images2");
        var repoImgDir = Path.Combine(Directory.GetCurrentDirectory(), "src", "F1.Web", "images2");
        for (int i = 1; i <= 20; i++)
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
