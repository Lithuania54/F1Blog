using System.ComponentModel.DataAnnotations;

namespace F1.Web.Models;

public class RaceResult
{
    public int Id { get; set; }

    public int RaceWeekendId { get; set; }
    public RaceWeekend RaceWeekend { get; set; } = null!;

    public int WinnerDriverId { get; set; }
    public Driver WinnerDriver { get; set; } = null!;

    public int SecondDriverId { get; set; }
    public Driver SecondDriver { get; set; } = null!;

    public int ThirdDriverId { get; set; }
    public Driver ThirdDriver { get; set; } = null!;

    public int? FastestLapDriverId { get; set; }
    public Driver? FastestLapDriver { get; set; }

    public bool SafetyCarDeployed { get; set; }
    public bool VirtualSafetyCarDeployed { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}
