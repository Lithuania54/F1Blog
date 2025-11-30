using System;

namespace F1.Web.Models;

public class NextRaceInfo
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTimeOffset StartTimeUtc { get; set; }
    public string Circuit { get; set; } = string.Empty;
    public string LocalStartDisplay { get; set; } = string.Empty;
    public string TrackMapUrl { get; set; } = string.Empty;
}
