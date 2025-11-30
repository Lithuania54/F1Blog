namespace F1.Web.Models;

public class PredictionLeaderboardEntry
{
    public string UserName { get; set; } = string.Empty;
    public int Points { get; set; }
    public int CorrectPodiums { get; set; }
    public int Streak { get; set; }
    public string Badge { get; set; } = string.Empty;
}
