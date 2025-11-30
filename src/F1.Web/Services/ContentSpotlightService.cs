using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using F1.Web.Data;
using F1.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace F1.Web.Services;

public interface IContentSpotlightService
{
    Task<IReadOnlyList<RaceRadioBite>> GetFeaturedRadioAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TechDrop>> GetFeaturedTechDropsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserBadge>> GetRecentBadgesAsync(int take = 6, CancellationToken cancellationToken = default);
}

public class ContentSpotlightService : IContentSpotlightService
{
    private readonly ApplicationDbContext _db;

    public ContentSpotlightService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RaceRadioBite>> GetFeaturedRadioAsync(CancellationToken cancellationToken = default)
    {
        var bites = await _db.RaceRadioBites
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        bites = bites
            .OrderByDescending(x => x.IsFeatured)
            .ThenByDescending(x => x.PublishedAt)
            .Take(6)
            .ToList();

        if (bites.Count == 0)
        {
            bites = new List<RaceRadioBite>
            {
                new RaceRadioBite{ Title="Team orders at Suzuka", RaceWeekendName="Japanese GP", Summary="Late-race radio asking to hold position.", SourceUrl="https://www.formula1.com/", ClipUrl="", IsFeatured=true },
                new RaceRadioBite{ Title="Driver reports tyre deg", RaceWeekendName="Bahrain GP", Summary="Front-left graining after 12 laps on C2.", SourceUrl="https://www.formula1.com/" }
            };
        }
        return bites;
    }

    public async Task<IReadOnlyList<TechDrop>> GetFeaturedTechDropsAsync(CancellationToken cancellationToken = default)
    {
        var drops = await _db.TechDrops
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        drops = drops
            .OrderByDescending(x => x.IsFeatured)
            .ThenByDescending(x => x.PublishedAt)
            .Take(6)
            .ToList();

        if (drops.Count == 0)
        {
            drops = new List<TechDrop>
            {
                new TechDrop{ Title="Ferrari rear wing tweak", TeamName="Ferrari", RaceWeekendName="Monza", Summary="Lower drag flap with revised endplate slot.", ImpactTags="aero, top speed", ImageUrl="", IsFeatured=true },
                new TechDrop{ Title="McLaren floor edge update", TeamName="McLaren", RaceWeekendName="Silverstone", Summary="Reprofiled edge wing to stabilize rear.", ImpactTags="aero, stability", ImageUrl="" }
            };
        }

        return drops;
    }

    public async Task<IReadOnlyList<UserBadge>> GetRecentBadgesAsync(int take = 6, CancellationToken cancellationToken = default)
    {
        var badges = await _db.UserBadges
            .Include(x => x.Badge)
            .OrderByDescending(x => x.BadgeId)
            .Take(take)
            .ToListAsync(cancellationToken);

        return badges;
    }
}
