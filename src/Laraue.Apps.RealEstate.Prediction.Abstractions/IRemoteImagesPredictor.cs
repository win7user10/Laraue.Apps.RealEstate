namespace Laraue.Apps.RealEstate.Prediction.Abstractions;

public interface IRemoteImagesPredictor
{
    Task<IDictionary<string, PredictionResult>> PredictAsync(
        IEnumerable<string> urls,
        CancellationToken ct = default);
}