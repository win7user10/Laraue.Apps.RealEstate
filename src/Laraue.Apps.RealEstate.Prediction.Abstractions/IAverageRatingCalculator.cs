namespace Laraue.Apps.RealEstate.Prediction.Abstractions;

public interface IAverageRatingCalculator
{
    /// <summary>
    /// Calculate rating for the passed predictions.
    /// </summary>
    /// <returns></returns>
    double? Calculate(IEnumerable<PredictionResult> predictionResults);
}