using System;
using System.ComponentModel.DataAnnotations;

namespace F1.Web.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        // The full post content (plain text / markdown)
        [Required]
        public string Content { get; set; } = string.Empty;

        // Comma separated hashtags (no leading #), e.g. "f1,monaco,qualy"
        public string? Hashtags { get; set; }

        // Optional cover image path/url (served from /images or external)
        public string? ImageUrl { get; set; }

        // UTC time when created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Identity user id
        public string? AuthorId { get; set; }

        // Snapshot of author display name at creation
        public string? AuthorName { get; set; }
    }
}
