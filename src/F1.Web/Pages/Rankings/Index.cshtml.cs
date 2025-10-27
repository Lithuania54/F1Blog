using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace F1.Web.Pages.Rankings
{
    public class TeamStanding
    {
        public int Position { get; set; }
        public string Team { get; set; } = string.Empty;
        public int Points { get; set; }
    }

    public class DriverStanding
    {
        public int Position { get; set; }
        public string Driver { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public int Points { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public List<TeamStanding> TeamStandings { get; set; } = new();
        public List<DriverStanding> DriverStandings { get; set; } = new();

        public IndexModel(ILogger<IndexModel> logger) => _logger = logger;

        public async Task OnGetAsync()
        {
            try
            {
                var dataFile = Path.Combine(Directory.GetCurrentDirectory(), "Data", "rankings-2025.json");

                if (System.IO.File.Exists(dataFile))
                {
                    var json = await System.IO.File.ReadAllTextAsync(dataFile);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (TryGetArray(root, out var teamsEl, "teams", "teamStandings"))
                    {
                        foreach (var el in teamsEl.EnumerateArray())
                        {
                            TeamStandings.Add(new TeamStanding
                            {
                                Position = GetInt(el, "position"),
                                Team = GetString(el, "team") ?? GetString(el, "name") ?? "",
                                Points = GetInt(el, "points")
                            });
                        }
                    }

                    if (TryGetArray(root, out var driversEl, "drivers", "driverStandings"))
                    {
                        foreach (var el in driversEl.EnumerateArray())
                        {
                            DriverStandings.Add(new DriverStanding
                            {
                                Position = GetInt(el, "position"),
                                Driver = GetString(el, "driver") ?? GetString(el, "name") ?? "",
                                Team = GetString(el, "team") ?? "",
                                Points = GetInt(el, "points")
                            });
                        }
                    }
                }

                if (TeamStandings.Count == 0)
                {
                    TeamStandings = new List<TeamStanding>
                    {
                        new TeamStanding { Position = 1, Team = "Red Bull Racing", Points = 700 },
                        new TeamStanding { Position = 2, Team = "Ferrari", Points = 450 },
                        new TeamStanding { Position = 3, Team = "Mercedes", Points = 380 }
                    };
                }

                if (DriverStandings.Count == 0)
                {
                    DriverStandings = new List<DriverStanding>
                    {
                        new DriverStanding { Position = 1, Driver = "Max Verstappen", Team = "Red Bull", Points = 410 },
                        new DriverStanding { Position = 2, Driver = "Lewis Hamilton", Team = "Mercedes", Points = 240 },
                        new DriverStanding { Position = 3, Driver = "Charles Leclerc", Team = "Ferrari", Points = 230 }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load rankings JSON - falling back to sample data.");
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

        private static string? GetString(JsonElement el, string propName)
        {
            if (el.TryGetProperty(propName, out var p) && p.ValueKind != JsonValueKind.Null)
                return p.GetString();
            return null;
        }

        private static int GetInt(JsonElement el, string propName)
        {
            if (el.TryGetProperty(propName, out var p))
            {
                if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var i)) return i;
                if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out var parsed)) return parsed;
            }
            return 0;
        }
    }
}
