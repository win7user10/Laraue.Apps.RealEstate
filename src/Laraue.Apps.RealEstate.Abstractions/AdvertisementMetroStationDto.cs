namespace Laraue.Apps.RealEstate.Abstractions;

public sealed class AdvertisementMetroStationDto
{
    public required string Name { get; init; }
    
    public required int DistanceInMinutes { get; init; }
    
    public required DistanceType DistanceType { get; init; }

    public required string Color { get; init; }
    
    public required long Id { get; init; }
}