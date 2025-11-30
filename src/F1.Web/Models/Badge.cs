using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace F1.Web.Models;

public class Badge
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(400)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(12)]
    public string Color { get; set; } = "#FFD200";

    [MaxLength(12)]
    public string AccentColor { get; set; } = "#000000";

    [MaxLength(200)]
    public string IconUrl { get; set; } = string.Empty;

    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}

public class UserBadge
{
    public int BadgeId { get; set; }
    public Badge Badge { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}
