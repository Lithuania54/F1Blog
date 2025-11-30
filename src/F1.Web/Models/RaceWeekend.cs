using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace F1.Web.Models;

public class RaceWeekend
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Country { get; set; } = string.Empty;

    [MaxLength(120)]
    public string CircuitName { get; set; } = string.Empty;

    [MaxLength(300)]
    public string TrackMapUrl { get; set; } = string.Empty;

    public DateTimeOffset RaceDateUtc { get; set; }
    public int Laps { get; set; }
    public double DistanceKm { get; set; }
    public int DRSZones { get; set; }

    [MaxLength(120)]
    public string TyreCompounds { get; set; } = string.Empty;

    [MaxLength(60)]
    public string TrackTimeZone { get; set; } = "UTC";

    [MaxLength(300)]
    public string WeatherSummary { get; set; } = string.Empty;

    public double AveragePitTimeSeconds { get; set; }

    public DateTimeOffset Fp1StartUtc { get; set; }
    public DateTimeOffset Fp2StartUtc { get; set; }
    public DateTimeOffset? Fp3StartUtc { get; set; }
    public DateTimeOffset QualifyingStartUtc { get; set; }
    public DateTimeOffset? SprintStartUtc { get; set; }
    public DateTimeOffset RaceStartUtc { get; set; }

    [MaxLength(2000)]
    public string PostRaceSummary { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public RaceResult? Result { get; set; }
    public ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();
    public ICollection<Upgrade> Upgrades { get; set; } = new List<Upgrade>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
