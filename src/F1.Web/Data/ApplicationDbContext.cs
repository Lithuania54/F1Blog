using F1.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace F1.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<RaceWeekend> RaceWeekends => Set<RaceWeekend>();
    public DbSet<RaceResult> RaceResults => Set<RaceResult>();
    public DbSet<Prediction> Predictions => Set<Prediction>();
    public DbSet<Upgrade> Upgrades => Set<Upgrade>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<UserBadge> UserBadges => Set<UserBadge>();
    public DbSet<RaceRadioBite> RaceRadioBites => Set<RaceRadioBite>();
    public DbSet<TechDrop> TechDrops => Set<TechDrop>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Team>()
            .HasMany(t => t.Drivers)
            .WithOne(d => d.Team)
            .HasForeignKey(d => d.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RaceWeekend>()
            .HasOne(r => r.Result)
            .WithOne(res => res.RaceWeekend)
            .HasForeignKey<RaceResult>(res => res.RaceWeekendId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RaceResult>()
            .HasOne(r => r.WinnerDriver)
            .WithMany()
            .HasForeignKey(r => r.WinnerDriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RaceResult>()
            .HasOne(r => r.SecondDriver)
            .WithMany()
            .HasForeignKey(r => r.SecondDriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RaceResult>()
            .HasOne(r => r.ThirdDriver)
            .WithMany()
            .HasForeignKey(r => r.ThirdDriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RaceResult>()
            .HasOne(r => r.FastestLapDriver)
            .WithMany()
            .HasForeignKey(r => r.FastestLapDriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Prediction>()
            .HasOne(p => p.User)
            .WithMany(u => u.Predictions)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Prediction>()
            .HasOne(p => p.RaceWeekend)
            .WithMany(r => r.Predictions)
            .HasForeignKey(p => p.RaceWeekendId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Prediction>()
            .HasOne(p => p.PredictedP1Driver)
            .WithMany()
            .HasForeignKey(p => p.PredictedP1DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Prediction>()
            .HasOne(p => p.PredictedP2Driver)
            .WithMany()
            .HasForeignKey(p => p.PredictedP2DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Prediction>()
            .HasOne(p => p.PredictedP3Driver)
            .WithMany()
            .HasForeignKey(p => p.PredictedP3DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Prediction>()
            .HasOne(p => p.PredictedFastestLapDriver)
            .WithMany()
            .HasForeignKey(p => p.PredictedFastestLapDriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Upgrade>()
            .HasOne(u => u.Team)
            .WithMany(t => t.Upgrades)
            .HasForeignKey(u => u.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Upgrade>()
            .HasOne(u => u.RaceWeekend)
            .WithMany(r => r.Upgrades)
            .HasForeignKey(u => u.RaceWeekendId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany()
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Comment>()
            .HasOne(c => c.RaceWeekend)
            .WithMany(r => r.Comments)
            .HasForeignKey(c => c.RaceWeekendId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserBadge>()
            .HasKey(ub => new { ub.BadgeId, ub.UserId });

        builder.Entity<UserBadge>()
            .HasOne(ub => ub.Badge)
            .WithMany(b => b.UserBadges)
            .HasForeignKey(ub => ub.BadgeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserBadge>()
            .HasOne(ub => ub.User)
            .WithMany(u => u.UserBadges)
            .HasForeignKey(ub => ub.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
