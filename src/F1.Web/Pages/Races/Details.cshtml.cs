using System;
using System.Collections.Generic;
using F1.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace F1.Web.Pages.Races;

public class DetailsModel : PageModel
{
    public class SessionInfo { public string Name { get; set; } = ""; public string TimeLocal { get; set; } = ""; }

    public NextRaceInfo Race { get; private set; } = new();
    public IReadOnlyList<SessionInfo> Sessions { get; private set; } = Array.Empty<SessionInfo>();
    public string WeatherSummary { get; private set; } = "Partly cloudy, light wind";
    public string Temperature { get; private set; } = "Air 23°C / Track 32°C";
    public string TyreAllocation { get; private set; } = "C2 (Hard), C3 (Medium), C4 (Soft)";

    public void OnGet()
    {
        // Placeholder sample; integrate with real race weekend data
        Race = new NextRaceInfo
        {
            Name = "Australian Grand Prix",
            Country = "Australia",
            Circuit = "Albert Park",
            StartTimeUtc = DateTimeOffset.UtcNow.AddDays(10).AddHours(5),
            LocalStartDisplay = "Sun 15:00 local",
            TrackMapUrl = ""
        };

        Sessions = new List<SessionInfo>
        {
            new() { Name = "FP1", TimeLocal = "Fri 12:30" },
            new() { Name = "FP2", TimeLocal = "Fri 16:00" },
            new() { Name = "FP3", TimeLocal = "Sat 12:00" },
            new() { Name = "Qualifying", TimeLocal = "Sat 15:00" },
            new() { Name = "Race", TimeLocal = "Sun 15:00" }
        };
    }
}
