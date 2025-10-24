namespace Blog.Core.Models
{
    public class AuthorProfile
    {
        public string Id { get; set; } = string.Empty; // matches IdentityUser.Id
        public string DisplayName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? AvatarPath { get; set; }
    }
}
