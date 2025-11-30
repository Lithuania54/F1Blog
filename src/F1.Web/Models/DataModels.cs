namespace F1.Web.Models;

// Legacy/sample records used for static page data; kept separate from domain entities.
public record DriverCard(string Id, string Name, string Team, string PhotoUrl, string Bio, int Wins, int Podiums);

public record RaceItem(string Id, string Name, string Location, DateTime Date, string MapUrl);

public record CaseStudy(string Id, string Title, string Excerpt, string ImageUrl, string Link);

public record SampleData(List<DriverCard> Drivers, List<RaceItem> Races, List<CaseStudy> Works);

