using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using F1.Web.Models;

namespace F1.Web.Pages.Races;

public class IndexModel : PageModel
{
    public List<RaceItem> Races { get; set; } = new();

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
        else
        {
            // fallback to sample-data.json races
            var sample = Path.Combine(Directory.GetCurrentDirectory(), "src", "F1.Web", "Data", "sample-data.json");
            if (System.IO.File.Exists(sample))
            {
                var doc = JsonSerializer.Deserialize<JsonElement>(System.IO.File.ReadAllText(sample));
                if (doc.TryGetProperty("Races", out var r))
                {
                    Races = JsonSerializer.Deserialize<List<RaceItem>>(r.GetRawText())!.OrderBy(x => x.Date).ToList();
                }
            }
        }
    }
}
