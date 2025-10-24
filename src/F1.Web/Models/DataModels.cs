namespace F1.Web.Models;

public record Driver(string Id, string Name, string Team, string PhotoUrl, string Bio, int Wins, int Podiums);

public record RaceItem(string Id, string Name, string Location, DateTime Date, string MapUrl);

public record CaseStudy(string Id, string Title, string Excerpt, string ImageUrl, string Link);

public record SampleData(List<Driver> Drivers, List<RaceItem> Races, List<CaseStudy> Works);
