using System;
using System.Linq;
using System.Threading.Tasks;
using Blog.Infrastructure;
using Blog.Core.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Blog.Tests
{
    public class EfPostRepositoryTests
    {
        [Fact]
        public async Task GetPublishedAsync_ReturnsPublishedOnly()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<BlogDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var db = new BlogDbContext(options))
            {
                db.Database.EnsureCreated();
                db.Posts.Add(new Post { Title = "P1", Slug = "p1", IsPublished = true, PublishedAt = DateTime.UtcNow });
                db.Posts.Add(new Post { Title = "P2", Slug = "p2", IsPublished = false });
                db.SaveChanges();
            }

            using (var db = new BlogDbContext(options))
            {
                var repo = new EfPostRepository(db);
                var published = await repo.GetPublishedAsync();
                Assert.Single(published);
                Assert.Equal("p1", published.First().Slug);
            }

            connection.Close();
        }
    }
}
