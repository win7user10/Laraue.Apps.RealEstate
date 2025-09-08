using Laraue.Apps.RealEstate.Abstractions;

namespace Laraue.Apps.RealEstate.Db.Models;

public sealed class CrawlingSession
{
    public long Id { get; init; }
    
    public DateTime StartedAt { get; init; }
    
    public DateTime FinishedAt { get; init; }

    /// <summary>
    /// Show which type of advertisement has been crawled in this session.
    /// </summary>
    public AdvertisementSource AdvertisementSource { get; init; }

    public ICollection<CrawlingSessionAdvertisement>? AffectedAdvertisements { get; init; }
}