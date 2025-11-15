using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;

namespace Laraue.Apps.RealEstate.Db.Models;

public sealed class Advertisement
{
    public long Id { get; set; }

    public string SourceId { get; set; } = string.Empty;
    
    public string Link { get; set; } = string.Empty!;
    
    public AdvertisementSource SourceType { get; set; }
    
    public decimal Square { get; set; }

    public int RoomsCount { get; set; }

    public int FloorNumber { get; set; }

    public int TotalFloorsNumber { get; set; }

    public string? ShortDescription { get; set; }
    
    public decimal TotalPrice { get; set; }

    public decimal SquareMeterPrice
    {
        get => Square > 0 ? TotalPrice / Square : 0;
        // ReSharper disable once ValueParameterNotUsed EF Core limitation
        private set {}
    }

    /// <summary>
    /// Date from the advertisement, when it was updated on the site.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Date when advertisement updated by crawler last time.
    /// </summary>
    public DateTime CrawledAt { get; set; }
    
    /// <summary>
    /// Date when advertisement was inserted into the service database first time.
    /// </summary>
    public DateTime FirstTimeCrawledAt { get; set; }
    
    /// <summary>
    /// When the prediction process is finished.
    /// </summary>
    public DateTime? PredictedAt { get; set; }
    
    /// <summary>
    /// When the item is ready to get via API.
    /// </summary>
    public DateTime? ReadyAt { get; set; }

    public int? RenovationRating { get; set; }
    public string[] Advantages { get; set; } = [];
    public string[] Problems { get; set; } = [];

    public decimal SquareMeterPredictedPrice { get; set; }

    public double Ideality { get; set; }

    public FlatType? FlatType { get; set; }

    public ICollection<AdvertisementTransportStop> TransportStops { get; init; } = new List<AdvertisementTransportStop>();
    
    public ICollection<AdvertisementImage> LinkedImages { get; init; } = new List<AdvertisementImage>();
}