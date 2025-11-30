using System;
using System.Threading;
using System.Threading.Tasks;
using F1.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace F1.Web.Pages.Admin.Standings;

[Authorize]
public class RefreshModel : PageModel
{
    private readonly IF1DataService _dataService;

    public RefreshModel(IF1DataService dataService)
    {
        _dataService = dataService;
    }

    public DateTimeOffset? LastRun { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await _dataService.RefreshStandingsAsync(cancellationToken);
        LastRun = DateTimeOffset.UtcNow;
        return Page();
    }
}
