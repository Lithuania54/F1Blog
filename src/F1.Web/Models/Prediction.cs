using System;
using System.ComponentModel.DataAnnotations;

namespace F1.Web.Models;

public class Prediction
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int RaceWeekendId { get; set; }
    public RaceWeekend RaceWeekend { get; set; } = null!;

    [Required]
    public int PredictedP1DriverId { get; set; }
    public Driver PredictedP1Driver { get; set; } = null!;

    [Required]
    public int PredictedP2DriverId { get; set; }
    public Driver PredictedP2Driver { get; set; } = null!;

    [Required]
    public int PredictedP3DriverId { get; set; }
    public Driver PredictedP3Driver { get; set; } = null!;

    public int? PredictedFastestLapDriverId { get; set; }
    public Driver? PredictedFastestLapDriver { get; set; }

    public bool PredictedSafetyCar { get; set; }

    public int Score { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
