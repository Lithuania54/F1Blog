using System.ComponentModel.DataAnnotations;

namespace F1.Web.Models;

public class Driver
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 99)]
    public int Number { get; set; }

    [MaxLength(60)]
    public string Nationality { get; set; } = string.Empty;

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    [MaxLength(2000)]
    public string Bio { get; set; } = string.Empty;

    public int DebutYear { get; set; }
    public int Championships { get; set; }
    public int Podiums { get; set; }
    public int Wins { get; set; }

    [MaxLength(300)]
    public string PhotoUrl { get; set; } = string.Empty;

    [MaxLength(300)]
    public string HelmetImageUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
