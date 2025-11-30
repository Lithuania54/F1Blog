using System;
using System.ComponentModel.DataAnnotations;

namespace F1.Web.Models;

public class TechDrop
{
    public int Id { get; set; }

    [Required, MaxLength(140)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(80)]
    public string TeamName { get; set; } = string.Empty;

    [MaxLength(120)]
    public string RaceWeekendName { get; set; } = string.Empty;

    [MaxLength(400)]
    public string Summary { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Details { get; set; } = string.Empty;

    [MaxLength(300)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(200)]
    public string ImpactTags { get; set; } = string.Empty; // e.g. "aero,+0.15s/ lap"

    [MaxLength(400)]
    public string LinkUrl { get; set; } = string.Empty; // official article or source

    public bool IsFeatured { get; set; }

    public DateTimeOffset PublishedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
