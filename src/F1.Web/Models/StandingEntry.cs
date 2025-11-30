namespace F1.Web.Models;

public class StandingEntry
{
    public int Position { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Wins { get; set; }
    public int Podiums { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string AccentColor { get; set; } = "#f85b60";
}
