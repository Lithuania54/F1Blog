using System;
using System.ComponentModel.DataAnnotations;

namespace F1.Web.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Hashtags { get; set; }   // stored as "tag1,tag2,tag3"
    [Required]
    public string? AuthorName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
