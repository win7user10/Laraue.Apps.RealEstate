namespace Laraue.Apps.RealEstate.Prediction.AppServices;

public record PredictionResult
{
    public int RenovationRating { get; init; }
    public string[] Advantages { get; init; } = [];
    public string[] Problems { get; init; } = [];
}