namespace Laraue.Apps.RealEstate.Prediction.Abstractions;

public interface IRemoteImagesPredictor
{
    Task<PredictionResult> PredictAsync(
        IEnumerable<string> urls,
        CancellationToken ct = default);
}