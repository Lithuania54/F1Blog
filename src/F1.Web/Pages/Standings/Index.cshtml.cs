using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using F1.Web.Models;
using F1.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace F1.Web.Pages.Standings;

public class IndexModel : PageModel
{
    private readonly IF1DataService _dataService;

    public IReadOnlyList<StandingEntry> DriverStandings { get; private set; } = Array.Empty<StandingEntry>();
    public IReadOnlyList<StandingEntry> TeamStandings { get; private set; } = Array.Empty<StandingEntry>();

    public IndexModel(IF1DataService dataService)
    {
        _dataService = dataService;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        DriverStandings = await _dataService.GetDriverStandingsAsync(cancellationToken);
        TeamStandings = await _dataService.GetConstructorStandingsAsync(cancellationToken);
    }
}
