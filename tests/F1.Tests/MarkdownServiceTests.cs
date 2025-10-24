using Xunit;
using F1.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace F1.Tests;

public class MarkdownServiceTests
{
    [Fact]
    public void LoadsSamplePost()
    {
        var env = new TestEnv();
        var svc = new MarkdownService(env);
        var posts = svc.GetAllPosts().ToList();
        // at least one sample post exists
        Assert.True(posts.Count >= 1);
    }

    class TestEnv : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "F1.Web.Tests";
        public string WebRootPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        public IFileProvider WebRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Directory.GetCurrentDirectory());
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory().Replace("tests", "src\\F1.Web");
        public IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Directory.GetCurrentDirectory());
    }
}
