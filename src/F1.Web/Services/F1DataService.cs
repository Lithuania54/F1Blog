using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using F1.Web.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace F1.Web.Services;

/// <summary>
/// Fetches standings data. Currently uses local JSON fallback and caches the result.
/// Swap the FetchLiveAsync methods to point to a permitted official F1 source when available.
/// </summary>
public class F1DataService : IF1DataService
{
    private readonly HttpClient _httpClient;
    private readonly IWebHostEnvironment _env;
    private readonly IMemoryCache _cache;
    private readonly ILogger<F1DataService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string DriverCacheKey = "f1:drivers";
    private const string TeamCacheKey = "f1:teams";
    private const string NextRaceCacheKey = "f1:nextrace";

    private static readonly Dictionary<string, string> DriverImages = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Max Verstappen", "/images3/verstappen.png" },
        { "Sergio Perez", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/sergio-perez/red-bull-racing.png" },
        { "Charles Leclerc", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/charles-leclerc/ferrari.png" },
        { "Carlos Sainz", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/carlos-sainz/ferrari.png" },
        { "Lando Norris", "/images3/norris.png" },
        { "Oscar Piastri", "/images3/piastri.png" },
        { "George Russell", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/george-russell/mercedes.png" },
        { "Lewis Hamilton", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/lewis-hamilton/mercedes.png" },
        { "Fernando Alonso", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/fernando-alonso/aston-martin.png" },
        { "Lance Stroll", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/lance-stroll/aston-martin.png" },
        { "Daniel Ricciardo", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/daniel-ricciardo/rb.png" },
        { "Yuki Tsunoda", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/yuki-tsunoda/rb.png" },
        { "Nico Hulkenberg", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/nico-hulkenberg/haas.png" },
        { "Kevin Magnussen", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/kevin-magnussen/haas.png" },
        { "Alexander Albon", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/alexander-albon/williams.png" },
        { "Logan Sargeant", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/logan-sargeant/williams.png" },
        { "Valtteri Bottas", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/valtteri-bottas/kick-sauber.png" },
        { "Zhou Guanyu", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/zhou-guanyu/kick-sauber.png" },
        { "Pierre Gasly", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/pierre-gasly/alpine.png" },
        { "Esteban Ocon", "https://media.formula1.com/content/dam/fom-website/drivers/2024Drivers/esteban-ocon/alpine.png" }
    };

    private static readonly Dictionary<string, string> TeamImages = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Red Bull Racing", "https://media.formula1.com/content/dam/fom-website/teams/2024/red-bull-racing.png" },
        { "Ferrari", "https://media.formula1.com/content/dam/fom-website/teams/2024/ferrari.png" },
        { "McLaren", "https://media.formula1.com/content/dam/fom-website/teams/2024/mclaren.png" },
        { "Mercedes", "https://media.formula1.com/content/dam/fom-website/teams/2024/mercedes.png" },
        { "Aston Martin", "https://media.formula1.com/content/dam/fom-website/teams/2024/aston-martin.png" },
        { "Alpine", "https://media.formula1.com/content/dam/fom-website/teams/2024/alpine.png" },
        { "RB", "https://media.formula1.com/content/dam/fom-website/teams/2024/alphatauri.png" },
        { "Haas", "https://media.formula1.com/content/dam/fom-website/teams/2024/haas-f1-team.png" },
        { "Williams", "https://media.formula1.com/content/dam/fom-website/teams/2024/williams.png" },
        { "Sauber", "https://media.formula1.com/content/dam/fom-website/teams/2024/kick-sauber.png" }
    };

    public F1DataService(HttpClient httpClient, IWebHostEnvironment env, IMemoryCache cache, ILogger<F1DataService> logger)
    {
        _httpClient = httpClient;
        _env = env;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<StandingEntry>> GetDriverStandingsAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(DriverCacheKey, out IReadOnlyList<StandingEntry>? cached) && cached != null)
            return cached;

        var data = await LoadFromOfficialSiteAsync(isDriver: true, cancellationToken)
                   ?? await LoadFromLocalAsync(isDriver: true, cancellationToken)
                   ?? GetSampleDrivers();
        data = BackfillImages(data, isDriver: true);
        _cache.Set(DriverCacheKey, data, TimeSpan.FromMinutes(5));
        return data;
    }

    public async Task<IReadOnlyList<StandingEntry>> GetConstructorStandingsAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(TeamCacheKey, out IReadOnlyList<StandingEntry>? cached) && cached != null)
            return cached;

        var data = await LoadFromOfficialSiteAsync(isDriver: false, cancellationToken)
                   ?? await LoadFromLocalAsync(isDriver: false, cancellationToken)
                   ?? GetSampleTeams();
        data = BackfillImages(data, isDriver: false);
        _cache.Set(TeamCacheKey, data, TimeSpan.FromMinutes(5));
        return data;
    }

    public async Task<NextRaceInfo> GetNextRaceAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(NextRaceCacheKey, out NextRaceInfo? cached) && cached != null)
            return cached;

        var info = await LoadNextRaceFromOfficialSiteAsync(cancellationToken) ?? GetSampleNextRace();
        _cache.Set(NextRaceCacheKey, info, TimeSpan.FromMinutes(10));
        return info;
    }

    public async Task RefreshStandingsAsync(CancellationToken cancellationToken = default)
    {
        _cache.Remove(DriverCacheKey);
        _cache.Remove(TeamCacheKey);
        await GetDriverStandingsAsync(cancellationToken);
        await GetConstructorStandingsAsync(cancellationToken);
    }

    /// <summary>
    /// Attempts to scrape the official Formula1 results pages for the current season.
    /// Drivers: https://www.formula1.com/en/results.html/{year}/drivers.html
    /// Teams:   https://www.formula1.com/en/results.html/{year}/team.html
    /// </summary>
    private async Task<IReadOnlyList<StandingEntry>?> LoadFromOfficialSiteAsync(bool isDriver, CancellationToken cancellationToken)
    {
        try
        {
            var year = DateTime.UtcNow.Year;
            var url = isDriver
                ? $"https://www.formula1.com/en/results.html/{year}/drivers.html"
                : $"https://www.formula1.com/en/results.html/{year}/team.html";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; F1DataService/1.0)");
            var response = await _httpClient.SendAsync(req, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var doc = new HtmlDocument();
            doc.Load(stream);

            var table = doc.DocumentNode.SelectSingleNode("//table[contains(@class,'resultsarchive-table')]/tbody");
            if (table == null) return null;

            var entries = new List<StandingEntry>();
            foreach (var row in table.SelectNodes("./tr") ?? Enumerable.Empty<HtmlNode>())
            {
                var cells = row.SelectNodes("./td");
                if (cells == null || cells.Count < 4) continue;

                var positionText = cells[0].InnerText.Trim();
                var pos = int.TryParse(positionText, out var p) ? p : entries.Count + 1;

                if (isDriver)
                {
                    var name = Clean(cells[1].InnerText);
                    var team = Clean(cells[2].InnerText);
                    var points = SafeInt(cells.Last().InnerText);
                    var imageUrl = BuildOfficialImageUrl(name, team, isDriver: true);
                    entries.Add(new StandingEntry
                    {
                        Position = pos,
                        Name = name,
                        Team = team,
                        Points = points,
                        Wins = 0,
                        Podiums = 0,
                        ImageUrl = imageUrl,
                        AccentColor = "#f85b60"
                    });
                }
                else
                {
                    var name = Clean(cells[1].InnerText);
                    var points = SafeInt(cells.Last().InnerText);
                    var imageUrl = BuildOfficialImageUrl(name, string.Empty, isDriver: false);
                    entries.Add(new StandingEntry
                    {
                        Position = pos,
                        Name = name,
                        Team = string.Empty,
                        Points = points,
                        Wins = 0,
                        Podiums = 0,
                        ImageUrl = imageUrl,
                        AccentColor = "#7a5af8"
                    });
                }
            }

            return entries.Count > 0 ? entries : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to scrape official F1 site.");
            return null;
        }
    }

    private async Task<IReadOnlyList<StandingEntry>?> LoadFromLocalAsync(bool isDriver, CancellationToken cancellationToken)
    {
        var candidates = new[]
        {
            Path.Combine(_env.ContentRootPath, "Data", "rankings-2025.json"),
            Path.Combine(_env.ContentRootPath, "src", "F1.Web", "Data", "rankings-2025.json")
        };

        var dataFile = candidates.FirstOrDefault(File.Exists);
        if (dataFile == null)
            return null;

        try
        {
            await using var stream = File.OpenRead(dataFile);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = doc.RootElement;

            var keySet = isDriver
                ? new[] { "drivers", "driverStandings", "DriverStandings" }
                : new[] { "teams", "teamStandings", "TeamStandings" };

            if (!TryGetArray(root, out var arr, keySet))
                return null;

            var result = new List<StandingEntry>();
            foreach (var el in arr.EnumerateArray())
            {
                result.Add(new StandingEntry
                {
                    Position = GetInt(el, "position", "Position"),
                    Name = GetString(el, isDriver ? "driver" : "team", "Driver", "Team", "name", "Name") ?? string.Empty,
                    Team = isDriver ? GetString(el, "team", "Team") ?? string.Empty : string.Empty,
                    Points = GetInt(el, "points", "Points"),
                    Wins = GetInt(el, "wins", "Wins"),
                    Podiums = GetInt(el, "podiums", "Podiums"),
                    ImageUrl = GetString(el, "imageUrl", "ImageUrl") ?? BuildPlaceholder(isDriver, GetString(el, isDriver ? "driver" : "team", "name") ?? "F1"),
                    AccentColor = isDriver ? "#f85b60" : "#7a5af8"
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load standings from local file {File}", dataFile);
            return null;
        }
    }

    private static IReadOnlyList<StandingEntry> GetSampleDrivers() => new[]
    {
        new StandingEntry{ Position=1, Name="Lando Norris", Team="McLaren", Points=357, Wins=5, Podiums=13, ImageUrl=BuildOfficialImageUrl("Lando Norris","McLaren", true), AccentColor="#f85b60"},
        new StandingEntry{ Position=2, Name="Max Verstappen", Team="Red Bull Racing", Points=344, Wins=6, Podiums=12, ImageUrl=BuildOfficialImageUrl("Max Verstappen","Red Bull Racing", true), AccentColor="#f85b60"},
        new StandingEntry{ Position=3, Name="Charles Leclerc", Team="Ferrari", Points=301, Wins=3, Podiums=10, ImageUrl=BuildOfficialImageUrl("Charles Leclerc","Ferrari", true), AccentColor="#f85b60"},
    };

    private static IReadOnlyList<StandingEntry> GetSampleTeams() => new[]
    {
        new StandingEntry{ Position=1, Name="McLaren", Points=658, Wins=8, Podiums=20, ImageUrl=BuildOfficialImageUrl("McLaren", string.Empty, false), AccentColor="#7a5af8"},
        new StandingEntry{ Position=2, Name="Red Bull Racing", Points=612, Wins=7, Podiums=19, ImageUrl=BuildOfficialImageUrl("Red Bull Racing", string.Empty, false), AccentColor="#7a5af8"},
        new StandingEntry{ Position=3, Name="Ferrari", Points=577, Wins=5, Podiums=17, ImageUrl=BuildOfficialImageUrl("Ferrari", string.Empty, false), AccentColor="#7a5af8"},
    };

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

    private static string BuildPlaceholder(bool isDriver, string name)
    {
        var label = Uri.EscapeDataString(name);
        var bg = isDriver ? "ffe2dc" : "e2ddff";
        return $"https://dummyimage.com/320x320/{bg}/1f1a17.jpg&text={label}";
    }

    private static bool NeedsImage(string? url)
    {
        return string.IsNullOrWhiteSpace(url) || url.Contains("dummyimage.com", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<StandingEntry> BackfillImages(IReadOnlyList<StandingEntry> entries, bool isDriver)
    {
        foreach (var entry in entries)
        {
            if (NeedsImage(entry.ImageUrl))
            {
                entry.ImageUrl = BuildOfficialImageUrl(entry.Name, entry.Team, isDriver);
            }
        }
        return entries;
    }

    private async Task<NextRaceInfo?> LoadNextRaceFromOfficialSiteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var year = DateTime.UtcNow.Year;
            var url = $"https://www.formula1.com/en/racing/{year}.html";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; F1DataService/1.0)");
            var response = await _httpClient.SendAsync(req, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var doc = new HtmlDocument();
            doc.Load(stream);

            // Heuristic: first event card that is not completed
            var eventNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'event-item') and contains(@class,'upcoming')]");
            if (eventNode == null)
            {
                // alternative selector
                var nodes = doc.DocumentNode.SelectNodes("//a[contains(@class,'event-item')]");
                eventNode = nodes?.FirstOrDefault(x => x.InnerText.Contains("Round", StringComparison.OrdinalIgnoreCase));
            }

            if (eventNode == null)
                return null;

            string name = Clean(eventNode.SelectSingleNode(".//div[contains(@class,'event-title')]")?.InnerText ?? "Next Grand Prix");
            string country = Clean(eventNode.SelectSingleNode(".//div[contains(@class,'event-country')]")?.InnerText ?? string.Empty);
            string when = Clean(eventNode.SelectSingleNode(".//div[contains(@class,'event-dates')]")?.InnerText ?? string.Empty);

            DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(7);
            if (DateTimeOffset.TryParse(when, out var parsed))
                start = parsed;

            return new NextRaceInfo
            {
                Name = name,
                Country = country,
                StartTimeUtc = start,
                Circuit = string.Empty,
                LocalStartDisplay = when,
                TrackMapUrl = string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to scrape next race info.");
            return null;
        }
    }

    private static NextRaceInfo GetSampleNextRace() => new()
    {
        Name = "Australian Grand Prix",
        Country = "Australia",
        StartTimeUtc = DateTimeOffset.UtcNow.AddDays(12).AddHours(5),
        Circuit = "Albert Park",
        LocalStartDisplay = "Sun 15:00 local",
        TrackMapUrl = ""
    };

    private static string Clean(string input)
    {
        return HtmlEntity.DeEntitize(input).Replace("\n", " ").Replace("\r", " ").Trim();
    }

    private static int SafeInt(string input)
    {
        return int.TryParse(input.Trim(), out var val) ? val : 0;
    }

    private static string BuildOfficialImageUrl(string name, string team, bool isDriver)
    {
        if (isDriver && DriverImages.TryGetValue(name, out var driverUrl))
            return driverUrl;
        if (!isDriver && TeamImages.TryGetValue(name, out var teamUrl))
            return teamUrl;
        if (isDriver && !string.IsNullOrWhiteSpace(team) && TeamImages.TryGetValue(team, out var teamBadge))
            return teamBadge;
        return BuildPlaceholder(isDriver: isDriver, name);
    }
}
