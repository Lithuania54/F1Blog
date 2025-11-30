using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace F1.Web.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}
