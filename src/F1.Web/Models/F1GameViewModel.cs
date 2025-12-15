using System;

namespace F1.Web.Models;

public class BestResultViewModel
{
    public double? BestLapTime { get; set; }

    public double? TotalTime { get; set; }

    public string? TrackKey { get; set; }

    public string? TrackName { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class F1GameViewModel
{
    public bool IsAuthenticated { get; set; }

    public BestResultViewModel? BestResult { get; set; }
}
