using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace F1.Web.Pages.Rankings;

public class IndexModel : PageModel
{
    public class Standing { public int Position {get;set;} public string Name {get;set;} = ""; public string Team {get;set;} = ""; public int Points {get;set;} public int Wins {get;set;} public int Podiums {get;set;} }

    public List<Standing> DriverStandings { get; set; } = new();
    public List<Standing> TeamStandings { get; set; } = new();

    public void OnGet()
    {
        var dataFile = Path.Combine(Directory.GetCurrentDirectory(), "src", "F1.Web", "Data", "rankings-2025.json");
        if (System.IO.File.Exists(dataFile))
        {
            var doc = JsonSerializer.Deserialize<JsonElement>(System.IO.File.ReadAllText(dataFile));
            if (doc.TryGetProperty("DriverStandings", out var d))
            {
                DriverStandings = JsonSerializer.Deserialize<List<Standing>>(d.GetRawText())!;
            }
            if (doc.TryGetProperty("TeamStandings", out var t))
            {
                TeamStandings = JsonSerializer.Deserialize<List<Standing>>(t.GetRawText())!;
            }
        }
    }
}
