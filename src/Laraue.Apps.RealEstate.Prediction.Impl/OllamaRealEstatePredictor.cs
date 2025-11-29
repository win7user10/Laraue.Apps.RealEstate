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
Provide exactly 1-10 unique features.
Approximate ranges for the advertisement: 
10 - Luxury
8-9 - Very good flat. Ready for live.
6-7 - The flat that requires non-capital renovation.
5 - Above normal. Abrasions, cheap or very old materials. But enough clean to live.
3-4 - Requires strong renovation. Is not ready for live.
1-2 - Damaged flat almost without renovation
0 - When impossible to determine interior, or photo contains not enough details

Important Notes:
Return HasNoRenovation = true when an interior space appears to be in the process of renovation or construction.
Pay attention to the house photos. Panel house is worse than made from bricks.
One feature length should be no more than 100 symbols.

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
    public bool HasNoRenovation { get; init; }
    public double RenovationRating { get; init; }
    public Feature[] Features { get; init; } = [];
}

public class Feature
{
    public required string Description { get; init; }
    public bool IsPositive { get; init; }
}