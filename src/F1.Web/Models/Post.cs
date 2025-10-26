using System;
using System.ComponentModel.DataAnnotations;

namespace F1.Web.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        // Comma-separated hashtags, e.g. "f1,monaco,tesla"
        public string? Hashtags { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? AuthorId { get; set; } // IdentityUser.Id

        // Optional: if you want to store the display name at creation
        public string? AuthorName { get; set; }
    }
}
