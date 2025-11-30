using System;
using System.ComponentModel.DataAnnotations;

namespace F1.Web.Models;

public class RaceRadioBite
{
    public int Id { get; set; }

    [Required, MaxLength(140)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(120)]
    public string RaceWeekendName { get; set; } = string.Empty;

    [MaxLength(400)]
    public string Summary { get; set; } = string.Empty;

    [MaxLength(400)]
    public string SourceUrl { get; set; } = string.Empty; // official article

    [MaxLength(400)]
    public string ClipUrl { get; set; } = string.Empty; // audio/video snippet

    public bool IsFeatured { get; set; }

    public DateTimeOffset PublishedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
