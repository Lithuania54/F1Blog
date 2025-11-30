using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using F1.Web.Models;

namespace F1.Web.Pages.Predictions;

public class LeaderboardModel : PageModel
{
    public List<PredictionLeaderboardEntry> Entries { get; private set; } = new();

    public void OnGet()
    {
        // Placeholder sample; replace with real prediction scoring logic
        Entries = new List<PredictionLeaderboardEntry>
        {
            new() { UserName = "gridmaster", Points = 128, CorrectPodiums = 6, Streak = 4, Badge = "Hot Streak" },
            new() { UserName = "undercutking", Points = 117, CorrectPodiums = 5, Streak = 2, Badge = "Consistent" },
            new() { UserName = "aerowizard", Points = 102, CorrectPodiums = 4, Streak = 3, Badge = "Tech Nerd" }
        };
    }
}
