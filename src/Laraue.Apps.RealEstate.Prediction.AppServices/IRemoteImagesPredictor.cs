namespace Laraue.Apps.RealEstate.Prediction.AppServices;

public interface IRemoteImagesPredictor
{
    Task<PredictionResult> PredictAsync(
        IEnumerable<string> urls,
        CancellationToken ct = default);
}