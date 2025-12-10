using Laraue.Apps.RealEstate.Contracts;
using Laraue.Apps.RealEstate.Prediction.Abstractions;

namespace Laraue.Apps.RealEstate.Prediction.Impl;

public sealed class AdvertisementComputedFieldsCalculator : IAdvertisementComputedFieldsCalculator
{
    public ComputedFields ComputeFields(AdvertisementData advertisement)
    {
        var squareMeterPrice = advertisement.Square > 0
            ? advertisement.TotalPrice / advertisement.Square
            : 0;
        
        var squareMeterPredictedFine = GetPredictedSquareMeterPriceFineCoef(advertisement);
        var squareMeterPredictedPrice = squareMeterPrice +
            (decimal)squareMeterPredictedFine * squareMeterPrice;
        
        var ideality = squareMeterPredictedPrice != 0
            ? squareMeterPrice / squareMeterPredictedPrice
            : 0;

        return new ComputedFields(squareMeterPredictedPrice, (double)ideality);
    }
    
    private double GetPredictedSquareMeterPriceFineCoef(AdvertisementData advertisementData)
    {
        // ~ 0.55 fine max for the bad metro station
        var transportFines = advertisementData.TransportStops
            .Select(x =>
                GetTransportStopDistanceFineCoef(x.DistanceInMinutes, x.DistanceType)
                + GetTransportStopPlacementFine(x.Priority))
            .ToList();

        const double metroIsNotNearFine = 1.0;

        var fine = GetRenovationRatingFineCoef(advertisementData.RenovationRating.GetValueOrDefault())
            + GetFloorNumberFineCoef(
                advertisementData.FloorNumber,
                advertisementData.TotalFloorsNumber)
            + (transportFines.Count > 0 ? transportFines.Min() : metroIsNotNearFine);
        
        return fine;
    }
    
    private static double GetRenovationRatingFineCoef(int renovationRating)
    {
        // When no renovation made fine is better that then it is var bad.
        if (renovationRating == 0)
        {
            return 0.3;
        }
        
        return 1 - renovationRating / (double)10;
    }
    
    private static double GetFloorNumberFineCoef(int floorNumber, int totalFloorsNumber)
    {
        return floorNumber == totalFloorsNumber
               || floorNumber == 1
            ? 0.2
            : 0;
    }
    
    /// <summary>
    /// Bad station = 0.3
    /// </summary>
    /// <param name="transportStopPriority"></param>
    /// <returns></returns>
    private static double GetTransportStopPlacementFine(
        int transportStopPriority)
    {
        return (transportStopPriority - 1) * 0.1;
    }
    
    /// <summary>
    /// 25 min walk = 0.25
    /// </summary>
    private static double GetTransportStopDistanceFineCoef(
        int distanceInMinutes,
        DistanceType distanceType)
    {
        const int minTimeToFine = 5;
        var walkDistanceTime = distanceInMinutes * (distanceType == DistanceType.Foot ? 1 : 2);
        if (walkDistanceTime <= minTimeToFine)
        {
            return 0;
        }

        return (walkDistanceTime - minTimeToFine) * 0.01;
    }
}