using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using F1.Web.Models;

namespace F1.Web.Services;

public interface IF1DataService
{
    Task<IReadOnlyList<StandingEntry>> GetDriverStandingsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StandingEntry>> GetConstructorStandingsAsync(CancellationToken cancellationToken = default);
}
