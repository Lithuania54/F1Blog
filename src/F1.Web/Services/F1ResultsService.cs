using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using F1.Web.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace F1.Web.Services;

public class F1ResultsService : IF1ResultsService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<F1ResultsService> _logger;

    private static readonly Uri DriversUri = new("https://www.formula1.com/en/results/2025/drivers");
    private static readonly Uri TeamsUri = new("https://www.formula1.com/en/results/2025/team");
    private const string DriversCacheKey = "f1:results:drivers";
    private const string TeamsCacheKey = "f1:results:teams";

    public F1ResultsService(HttpClient httpClient, IMemoryCache cache, ILogger<F1ResultsService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        _cache.Remove(DriversCacheKey);
        _cache.Remove(TeamsCacheKey);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<StandingEntry>> GetDriverStandingsAsync(CancellationToken cancellationToken = default) =>
        GetStandingsAsync(isDriver: true, cancellationToken);

    public Task<IReadOnlyList<StandingEntry>> GetConstructorStandingsAsync(CancellationToken cancellationToken = default) =>
        GetStandingsAsync(isDriver: false, cancellationToken);

    private async Task<IReadOnlyList<StandingEntry>> GetStandingsAsync(bool isDriver, CancellationToken cancellationToken)
    {
        var cacheKey = isDriver ? DriversCacheKey : TeamsCacheKey;
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<StandingEntry>? cached) && cached != null)
        {
            return cached;
        }

        try
        {
            var uri = isDriver ? DriversUri : TeamsUri;
            var html = await FetchAsync(uri, cancellationToken);
            var parsed = ParseStandings(html, isDriver);
            _cache.Set(cacheKey, parsed, TimeSpan.FromMinutes(5));
            return parsed;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch live F1 standings ({Mode})", isDriver ? "drivers" : "teams");
            return Array.Empty<StandingEntry>();
        }
    }

    private async Task<string> FetchAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, uri);
        req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; F1ResultsService/1.0)");
        var response = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static IReadOnlyList<StandingEntry> ParseStandings(string html, bool isDriver)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html ?? string.Empty);
        var table = doc.DocumentNode.SelectSingleNode("//table");
        if (table == null)
            return Array.Empty<StandingEntry>();

        var rows = table.SelectNodes(".//tbody/tr") ?? Enumerable.Empty<HtmlNode>();
        var list = new List<StandingEntry>();

        foreach (var row in rows)
        {
            var cells = row.SelectNodes("./td");
            if (cells == null)
                continue;

            if (isDriver)
            {
                if (cells.Count < 5)
                    continue;

                var name = CleanDriverName(cells[1]);
                var team = Clean(cells[3].InnerText);
                var points = SafeInt(cells[^1].InnerText);
                var image = ExtractImage(cells[1]);

                list.Add(new StandingEntry
                {
                    Position = SafeInt(cells[0].InnerText),
                    Name = name,
                    Team = team,
                    Points = points,
                    Wins = 0,
                    Podiums = 0,
                    ImageUrl = image,
                    AccentColor = "#f85b60"
                });
            }
            else
            {
                if (cells.Count < 3)
                    continue;

                var name = Clean(cells[1].InnerText);
                var points = SafeInt(cells[2].InnerText);
                var image = ExtractImage(cells[1]);

                list.Add(new StandingEntry
                {
                    Position = SafeInt(cells[0].InnerText),
                    Name = name,
                    Team = string.Empty,
                    Points = points,
                    Wins = 0,
                    Podiums = 0,
                    ImageUrl = image,
                    AccentColor = "#7a5af8"
                });
            }
        }

        return list;
    }

    private static string ExtractImage(HtmlNode node)
    {
        var img = node.SelectSingleNode(".//img");
        return img?.GetAttributeValue("src", string.Empty) ?? string.Empty;
    }

    private static string CleanDriverName(HtmlNode cell)
    {
        var name = Clean(cell.InnerText);
        return Regex.Replace(name, "\\s+[A-Z]{3}$", string.Empty).Trim();
    }

    private static string Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        var text = HtmlEntity.DeEntitize(value);
        return Regex.Replace(text, "\\s+", " ").Trim();
    }

    private static int SafeInt(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0;
        var cleaned = Regex.Replace(input, "[^0-9]", "");
        return int.TryParse(cleaned, out var val) ? val : 0;
    }
}
