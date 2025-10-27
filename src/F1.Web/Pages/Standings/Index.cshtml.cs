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
        // Try to load standings from Data/rankings-2025.json with flexible key handling
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "Data", "rankings-2025.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "src", "F1.Web", "Data", "rankings-2025.json")
        };

        string? dataFile = Array.Find(candidates, System.IO.File.Exists);
        if (dataFile != null)
        {
            var json = System.IO.File.ReadAllText(dataFile);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Drivers: try keys "drivers", "driverStandings", "DriverStandings"
            if (TryGetArray(root, out var drivers, "drivers", "driverStandings", "DriverStandings"))
            {
                DriverStandings.Clear();
                foreach (var el in drivers.EnumerateArray())
                {
                    DriverStandings.Add(new Standing
                    {
                        Position = GetInt(el, "position", "Position"),
                        Name = GetString(el, "driver", "Driver", "name", "Name") ?? string.Empty,
                        Team = GetString(el, "team", "Team") ?? string.Empty,
                        Points = GetInt(el, "points", "Points"),
                        Wins = GetInt(el, "wins", "Wins"),
                        Podiums = GetInt(el, "podiums", "Podiums")
                    });
                }
            }

            // Teams: try keys "teams", "teamStandings", "TeamStandings"
            if (TryGetArray(root, out var teams, "teams", "teamStandings", "TeamStandings"))
            {
                TeamStandings.Clear();
                foreach (var el in teams.EnumerateArray())
                {
                    TeamStandings.Add(new Standing
                    {
                        Position = GetInt(el, "position", "Position"),
                        Name = GetString(el, "team", "Team", "name", "Name") ?? string.Empty,
                        Points = GetInt(el, "points", "Points"),
                        Wins = GetInt(el, "wins", "Wins"),
                        Podiums = GetInt(el, "podiums", "Podiums")
                    });
                }
            }
        }

        // If still empty (file missing or parse mismatch), set minimal placeholders
        if (DriverStandings.Count == 0)
        {
            DriverStandings = new List<Standing>
            {
                new Standing{Position=1,Name="Lando Norris",Team="McLaren",Points=357},
                new Standing{Position=2,Name="Oscar Piastri",Team="McLaren",Points=356},
                new Standing{Position=3,Name="Max Verstappen",Team="Red Bull Racing",Points=321}
            };
        }

        if (TeamStandings.Count == 0)
        {
            TeamStandings = new List<Standing>
            {
                new Standing{Position=1,Name="McLaren",Points=713},
                new Standing{Position=2,Name="Ferrari",Points=356},
                new Standing{Position=3,Name="Mercedes",Points=355}
            };
        }
    }

    private static bool TryGetArray(JsonElement root, out JsonElement arrayElement, params string[] candidateNames)
    {
        arrayElement = default;
        foreach (var n in candidateNames)
        {
            if (root.TryGetProperty(n, out var el) && el.ValueKind == JsonValueKind.Array)
            {
                arrayElement = el;
                return true;
            }
        }
        return false;
    }

    private static string? GetString(JsonElement el, params string[] names)
    {
        foreach (var n in names)
        {
            if (el.TryGetProperty(n, out var p) && p.ValueKind != JsonValueKind.Null)
            {
                return p.GetString();
            }
        }
        return null;
    }

    private static int GetInt(JsonElement el, params string[] names)
    {
        foreach (var n in names)
        {
            if (el.TryGetProperty(n, out var p))
            {
                if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var i)) return i;
                if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out var parsed)) return parsed;
            }
        }
        return 0;
    }
}
