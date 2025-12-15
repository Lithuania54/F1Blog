using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using F1.Web.Models;
using F1.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace F1.Web.Pages.Standings;

public class IndexModel : PageModel
{
    private readonly IF1ResultsService _resultsService;
    private readonly IF1DataService _dataService;

    public IReadOnlyList<StandingEntry> DriverStandings { get; private set; } = Array.Empty<StandingEntry>();
    public IReadOnlyList<StandingEntry> TeamStandings { get; private set; } = Array.Empty<StandingEntry>();
    public NextRaceInfo NextRace { get; private set; } = new();

    public IndexModel(IF1ResultsService resultsService, IF1DataService dataService)
    {
        _resultsService = resultsService;
        _dataService = dataService;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        DriverStandings = await _resultsService.GetDriverStandingsAsync(cancellationToken);
        TeamStandings = await _resultsService.GetConstructorStandingsAsync(cancellationToken);
        NextRace = await _dataService.GetNextRaceAsync(cancellationToken);
    }
}
