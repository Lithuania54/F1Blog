using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace F1.Web.Pages.Standings;

public class IndexModel : PageModel
{
    public class Standing { public int Position {get;set;} public string Name {get;set;} = ""; public string Team {get;set;} = ""; public int Points {get;set;} public int Wins {get;set;} public int Podiums {get;set;} }

    public List<Standing> DriverStandings { get; set; } = new();
    public List<Standing> TeamStandings { get; set; } = new();

    public void OnGet()
    {
        // Try to load sample data from Data/rankings-2025.json; if missing, populate with sample entries
        var dataFile = Path.Combine(Directory.GetCurrentDirectory(), "src", "F1.Web", "Data", "rankings-2025.json");
        if (System.IO.File.Exists(dataFile))
        {
            var doc = JsonSerializer.Deserialize<JsonElement>(System.IO.File.ReadAllText(dataFile));
            if (doc.TryGetProperty("DriverStandings", out var d)) DriverStandings = JsonSerializer.Deserialize<List<Standing>>(d.GetRawText())!;
            if (doc.TryGetProperty("TeamStandings", out var t)) TeamStandings = JsonSerializer.Deserialize<List<Standing>>(t.GetRawText())!;
        }
        else
        {
            // Generate placeholder realistic-ish data
            DriverStandings = new List<Standing>
            {
                new Standing{Position=1,Name="Max Verstappen",Team="Red Bull",Points=325,Wins=10,Podiums=12},
                new Standing{Position=2,Name="Lewis Hamilton",Team="Mercedes",Points=240,Wins=3,Podiums=8},
                new Standing{Position=3,Name="Charles Leclerc",Team="Ferrari",Points=200,Wins=2,Podiums=6}
            };
            TeamStandings = new List<Standing>
            {
                new Standing{Position=1,Name="Red Bull",Points=540,Wins=12},
                new Standing{Position=2,Name="Mercedes",Points=380,Wins=3},
                new Standing{Position=3,Name="Ferrari",Points=320,Wins=2}
            };
        }
    }
}
