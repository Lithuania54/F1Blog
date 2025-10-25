using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using F1.Web.Models;

namespace F1.Web.Pages.Races;

public class IndexModel : PageModel
{
    public List<RaceItem> Races { get; set; } = new();

    // Races are intentionally disabled for now per requirements. This page
    // shows a prominent placeholder. When enabled, load data from a JSON file
    // matching the schema in the TODO in the view.
    public void OnGet()
    {
    }
}
