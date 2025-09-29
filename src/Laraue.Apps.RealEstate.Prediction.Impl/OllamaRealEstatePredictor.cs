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
Analyze this image and determine if it depicts a flat interior, whether it has been renovated, and provide a renovation rating.
Formula of renovation rating: The value should equal to 0 when impossible to determine interior or photo contains not enough details to make classification.
Also it should be 0 when on the picture: view of the house outside, view of the yard, trees on photo, exterior view, etc
Approximate ranges: 
Value > 0.9 - Luxury
Value > 0.75 - Nice flat. Ready for live.
Value > 0.55 - The flat requires renovation, but it is possible. Someone can live here but without comfort.
Value > 0.35 - The flat requires renovation, it will be hard to made it. Flat is not ready for live.
Value < 0.35 - The flat renovation causes fear. Flat is dangerous for live.

Max value is 1.00.
The next things are matter when classifying
1. Flat has expensive interior materials (+).
2. Flat has furniture and it quality (+).
3. Flat is ready to live (+).
4. Empty room (-).
5. Mess on photo (-).
6. Design solutions (+).
7. Balanced color scheme (+).
8. Finished renovation (+).
9. The floor quality and nice materials (+)
10. Curtains, TV size and other (+)
Return as JSON.
```
{
    renovationRating: double value,
    tags: [Picture features, no more 50 symbols for each tag (no more than 5 tags)],
    description: Why this renovation rating was chosen (no more than 100 symbols)
}
```
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
    public double RenovationRating { get; init; }
    public string[] Tags { get; init; } = [];
    public string Description { get; init; } = string.Empty;
}

public record CheckTagsResult
{
    public bool IsApplicable { get; init; }
}