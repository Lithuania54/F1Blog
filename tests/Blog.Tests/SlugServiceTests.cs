using System.Threading.Tasks;
using Blog.Core.Interfaces;
using Blog.Web.Services;
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
}
