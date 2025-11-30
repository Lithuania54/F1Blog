using System.ComponentModel.DataAnnotations;

namespace F1.Web.Models;

public class Upgrade
{
    public int Id { get; set; }

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public int RaceWeekendId { get; set; }
    public RaceWeekend RaceWeekend { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Impact { get; set; } = string.Empty;
}
