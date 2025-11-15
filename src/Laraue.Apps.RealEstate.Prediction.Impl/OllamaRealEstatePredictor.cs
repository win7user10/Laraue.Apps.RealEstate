namespace Laraue.Apps.RealEstate.Prediction.Impl;

public interface IPredictor
{
    Task<OllamaPredictionResult> PredictAsync(string base64EncodedImage, CancellationToken ct = default);
}

public class OllamaRealEstatePredictor(
    IOllamaPredictor ollamaPredictor)
    : IPredictor
{
    public async Task<OllamaPredictionResult> PredictAsync(string base64EncodedImage, CancellationToken ct = default)
    {
        var promptImageAnalyze = @"
You are professional realtor and should estimate the advertisement images.
Analyze the passed collage where images are separated by black line, and calculates the renovation rating for the whole flat.
Approximate ranges for the advertisement: 
10 - Luxury
8-9 - Very good flat. Ready for live.
6-7 - The flat that requires non-capital renovation.
5 - Above normal. Abrasions, cheap or very old materials. But enough clean to live.
3-4 - Requires strong renovation. Is not ready for live.
1-2 - Damaged flat almost without renovation
0 - Rough finish, not ready for life or no relevant image

Important Notes:
Don't consider images that doesn't contain information's that helps to calculating rating.
The apartments in rough finish should return 0.

Return as JSON.
";
        
        var predictionResult = await ollamaPredictor.PredictAsync<OllamaPredictionResult>(
            "qwen2.5vl:7b",
            promptImageAnalyze,
            base64EncodedImage,
            ct);

        return predictionResult;
    }
}

public record OllamaPredictionResult
{
    public int RenovationRating { get; init; }
    public string[] Advantages { get; init; } = [];
    public string[] Problems { get; init; } = [];
}