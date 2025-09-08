namespace Laraue.Apps.RealEstate.Db.Models;

public sealed class CrawlingSessionAdvertisement
{
    public long CrawlingSessionId { get; init; }
    
    public CrawlingSession? CrawlingSession { get; init; }
    
    public long AdvertisementId { get; init; }
    
    public Advertisement? Advertisement { get; init; }
}