using Laraue.Apps.RealEstate.Abstractions;

namespace Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;

public interface IAdvertisementComputedFieldsCalculator
{
    ComputedFields ComputeFields(AdvertisementData advertisement);
}

public record struct ComputedFields(
    decimal SquareMeterPredictedPrice,
    double Ideality);

public sealed record AdvertisementData(
    decimal Square,
    decimal TotalPrice,
    int? RenovationRating,
    int FloorNumber,
    int TotalFloorsNumber,
    IEnumerable<TransportStopData> TransportStops);
    
public sealed record TransportStopData(
    int Priority,
    int DistanceInMinutes,
    DistanceType DistanceType);