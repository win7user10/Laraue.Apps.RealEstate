using Laraue.Apps.RealEstate.Prediction.Abstractions;

namespace Laraue.Apps.RealEstate.Prediction.Impl;

public sealed class AverageRatingCalculator : IAverageRatingCalculator
{
    private const int MinRelevantImagesForRelevant = 3;
    
    public double? Calculate(IEnumerable<PredictionResult> predictionResults)
    {
        var predictionResultsArray = predictionResults.ToArray();
        
        var relevantPredictions = predictionResultsArray
            .Where(x => x.IsPictureRelevant)
            .ToArray();

        if (relevantPredictions.Length < MinRelevantImagesForRelevant)
        {
            // Not enough images, high chance to get wrong prediction, return null rating
            return null;
        }

        return relevantPredictions
            .Select(x => x.RenovationRating)
            .Average();
    }
}