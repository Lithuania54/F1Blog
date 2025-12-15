using System;
using System.ComponentModel.DataAnnotations;

namespace F1.Web.Models;

public class UserBestResult
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public double BestLapTime { get; set; }

    public double? TotalTime { get; set; }

    [MaxLength(128)]
    public string? TrackKey { get; set; }

    [MaxLength(256)]
    public string? TrackName { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}
