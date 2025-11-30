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
        _cache.Set(DriverCacheKey, data, TimeSpan.FromMinutes(15));
        return data;
    }

    public async Task<IReadOnlyList<StandingEntry>> GetConstructorStandingsAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(TeamCacheKey, out IReadOnlyList<StandingEntry>? cached) && cached != null)
            return cached;

        var data = await LoadFromOfficialSiteAsync(isDriver: false, cancellationToken)
                   ?? await LoadFromLocalAsync(isDriver: false, cancellationToken)
                   ?? GetSampleTeams();
        _cache.Set(TeamCacheKey, data, TimeSpan.FromMinutes(15));
        return data;
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
                    var imageUrl = BuildOfficialImageUrl(name, isDriver: true);
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
                    var imageUrl = BuildOfficialImageUrl(name, isDriver: false);
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
        new StandingEntry{ Position=1, Name="Lando Norris", Team="McLaren", Points=357, Wins=5, Podiums=13, ImageUrl=BuildPlaceholder(true,"Lando Norris"), AccentColor="#f85b60"},
        new StandingEntry{ Position=2, Name="Max Verstappen", Team="Red Bull Racing", Points=344, Wins=6, Podiums=12, ImageUrl=BuildPlaceholder(true,"Max Verstappen"), AccentColor="#f85b60"},
        new StandingEntry{ Position=3, Name="Charles Leclerc", Team="Ferrari", Points=301, Wins=3, Podiums=10, ImageUrl=BuildPlaceholder(true,"Charles Leclerc"), AccentColor="#f85b60"},
    };

    private static IReadOnlyList<StandingEntry> GetSampleTeams() => new[]
    {
        new StandingEntry{ Position=1, Name="McLaren", Points=658, Wins=8, Podiums=20, ImageUrl=BuildPlaceholder(false,"McLaren"), AccentColor="#7a5af8"},
        new StandingEntry{ Position=2, Name="Red Bull Racing", Points=612, Wins=7, Podiums=19, ImageUrl=BuildPlaceholder(false,"Red Bull"), AccentColor="#7a5af8"},
        new StandingEntry{ Position=3, Name="Ferrari", Points=577, Wins=5, Podiums=17, ImageUrl=BuildPlaceholder(false,"Ferrari"), AccentColor="#7a5af8"},
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

    private static string Clean(string input)
    {
        return HtmlEntity.DeEntitize(input).Replace("\n", " ").Replace("\r", " ").Trim();
    }

    private static int SafeInt(string input)
    {
        return int.TryParse(input.Trim(), out var val) ? val : 0;
    }

    private static string BuildOfficialImageUrl(string name, bool isDriver)
    {
        // Best-effort: return official F1 logo for teams; driver placeholder otherwise.
        if (!isDriver)
            return "https://media.formula1.com/content/dam/fom-website/teams/2024/fia-f1-logo.png";

        return BuildPlaceholder(isDriver: true, name);
    }
}
