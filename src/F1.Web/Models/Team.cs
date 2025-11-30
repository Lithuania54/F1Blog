using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace F1.Web.Models;

public class Team
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(80)]
    public string BaseCountry { get; set; } = string.Empty;

    public int Championships { get; set; }

    [MaxLength(300)]
    public string LogoUrl { get; set; } = string.Empty;

    [MaxLength(12)]
    public string PrimaryColor { get; set; } = "#FF3B30";

    [MaxLength(12)]
    public string SecondaryColor { get; set; } = "#0B0B0B";

    [MaxLength(1000)]
    public string ShortHistory { get; set; } = string.Empty;

    public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
    public ICollection<Upgrade> Upgrades { get; set; } = new List<Upgrade>();
}
