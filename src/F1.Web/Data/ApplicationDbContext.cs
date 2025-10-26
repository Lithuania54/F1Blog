using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace F1.Web.Data
{
    // IdentityDbContext provides IdentityUser, roles, etc.
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Posts table for blogs
        public DbSet<F1.Web.Models.Post> Posts { get; set; } = null!;
    }
}
