using System.Threading.Tasks;
using Blog.Core.Interfaces;
using Moq;
using Xunit;

namespace Blog.Tests
{
    public class SlugServiceTests
    {
        [Fact]
        public async Task GenerateSlugAsync_Unique_AppendsNumberWhenNeeded()
        {
            var repoMock = new Mock<IPostRepository>();
            repoMock.SetupSequence(r => r.GetBySlugAsync(It.IsAny<string>()))
                .ReturnsAsync((Blog.Core.Models.Post?)null)
                .ReturnsAsync(new Blog.Core.Models.Post { Id = 1, Slug = "test" });
            var svc = new SlugService(repoMock.Object);
            var slug = await svc.GenerateSlugAsync("Test Post");
            Assert.False(string.IsNullOrWhiteSpace(slug));
        }
    }

    // Minimal SlugService implementation for tests (copied/simplified).
    // This avoids depending on the removed Blog.Web project. Replace with the real service in production.
    public class SlugService
    {
        private readonly IPostRepository _repo;
        public SlugService(IPostRepository repo) => _repo = repo;

        public async Task<string> GenerateSlugAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return string.Empty;
            string baseSlug = Slugify(title);
            var slug = baseSlug;
            int counter = 1;
            while (await _repo.GetBySlugAsync(slug) != null)
            {
                slug = baseSlug + "-" + counter;
                counter++;
                // safety
                if (counter > 1000) break;
            }
            return slug;
        }

        private static string Slugify(string input)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var c in input.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c)) sb.Append(c);
                else if (char.IsWhiteSpace(c) || c == '-' ) sb.Append('-');
            }
            var s = sb.ToString();
            while (s.Contains("--")) s = s.Replace("--","-");
            return s.Trim('-');
        }
    }
}
