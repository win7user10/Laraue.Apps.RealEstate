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

    public DateTime UpdatedAt { get; set; }
    
    public DateTime CrawledAt { get; set; }

    public double? RenovationRating { get; set; }

    public decimal SquareMeterPredictedPrice { get; set; }

    public double Ideality { get; set; }

    public FlatType? FlatType { get; set; }

    public void UpdateComputedFields(ComputedFields computedFields)
    {
        SquareMeterPrice = computedFields.SquareMeterPrice;
        SquareMeterPredictedPrice = computedFields.SquareMeterPredictedPrice;
        Ideality = computedFields.Ideality;
    }

    public ICollection<AdvertisementTransportStop> TransportStops { get; init; } = new List<AdvertisementTransportStop>();
    
    public ICollection<AdvertisementImage> LinkedImages { get; init; } = new List<AdvertisementImage>();
}