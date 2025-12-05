namespace Laraue.Apps.RealEstate.Abstractions;

public sealed record AdvertisementDto
{
    public required long Id { get; init; }
    
    public required string SourceId { get; init; } = default!;
    
    public required AdvertisementSource SourceType { get; init; }

    public required string Link { get; init; }

    public required decimal Square { get; init; }

    public required int RoomsCount { get; set; }

    public required int FloorNumber { get; init; }

    public required int TotalFloorsNumber { get; init; }
    
    public required decimal TotalPrice { get; init; }
    
    public required decimal SquareMeterPrice { get; init; }
    
    public required int? RenovationRating { get; init; }

    public required DateTime UpdatedAt { get; init; }
    public required DateTime CrawledAt { get; init; }
    public required DateTime FirstTimeCrawledAt { get; init; }
    public required string? ShortDescription { get; init; }
    public required string[] Advantages { get; init; }
    public required string[] Problems { get; init; }
    public required long? HouseId { get; init; }
    public required string? HouseNumber { get; init; }
    public required string? Address { get; init; }
    public required string? CityName { get; init; }
    public decimal? PredictedMarketPrice { get; set; }

    public required IEnumerable<AdvertisementMetroStationDto> MetroStations { get; init; }
    public required IEnumerable<AdvertisementImageDto> Images { get; init; }
}