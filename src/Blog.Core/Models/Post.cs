using System;
using System.Collections.Generic;

namespace Blog.Core.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty; // Markdown
        public string? Excerpt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool IsPublished { get; set; }
        public string? CoverImagePath { get; set; }

        // Relationships
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? AuthorId { get; set; } // Identity User Id
        public AuthorProfile? AuthorProfile { get; set; }

        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
