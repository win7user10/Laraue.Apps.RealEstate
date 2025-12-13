namespace Laraue.Apps.RealEstate.Prediction.AppServices;

public class PredictionOptions
{
    public required string Model { get; set; }
    public required string OllamaBaseAddress { get; set; }
    public required TimeSpan Timeout { get; set; }
}