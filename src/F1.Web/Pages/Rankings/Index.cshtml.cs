using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace F1.Web.Pages.Rankings;

public class IndexModel : PageModel
{
    public class Standing { public int Position {get;set;} public string Name {get;set;} = ""; public string Team {get;set;} = ""; public int Points {get;set;} public int Wins {get;set;} public int Podiums {get;set;} }

    public List<Standing> DriverStandings { get; set; } = new();
    public List<Standing> TeamStandings { get; set; } = new();

    // Rankings are intentionally disabled for now per requirements. This page
    // shows a prominent placeholder. When enabled, load data from a JSON file
    // matching the schema in the TODO in the view.
    public void OnGet()
    {
    }
}
