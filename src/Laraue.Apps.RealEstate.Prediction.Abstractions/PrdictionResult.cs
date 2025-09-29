namespace Laraue.Apps.RealEstate.Prediction.Abstractions;

public record PredictionResult
{
    public bool IsPictureRelevant => RenovationRating != 0;
    public double RenovationRating { get; init; }
    public string? Description { get; init; }
    public string[] Tags { get; init; } = [];
    public bool ErrorWhileRequesting { get; init; }
}