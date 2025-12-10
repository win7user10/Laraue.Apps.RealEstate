namespace Laraue.Apps.RealEstate.Crawling.Contracts.Crawler;

public record State
{
    public int LastPage { get; set; }
    
    public DateTime? StartedAt { get; set; }
    
    public DateTime? FinishedAt { get; set; }

    public HashSet<long> UpdatedAdvertisements { get; set; } = new();

    public DateTime MinUpdatedAt { get; set; }
}