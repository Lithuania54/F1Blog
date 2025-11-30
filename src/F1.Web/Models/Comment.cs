using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace F1.Web.Models;

public class Comment
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int? PostId { get; set; }
    public Post? Post { get; set; }

    public int? RaceWeekendId { get; set; }
    public RaceWeekend? RaceWeekend { get; set; }

    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
