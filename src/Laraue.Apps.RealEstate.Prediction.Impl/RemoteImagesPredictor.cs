using Laraue.Apps.RealEstate.Prediction.Abstractions;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace Laraue.Apps.RealEstate.Prediction.Impl;

public sealed class RemoteImagesPredictor : IRemoteImagesPredictor
{
    private readonly ILogger<RemoteImagesPredictor> _logger;
    private readonly IPredictor _predictor;
    private readonly HttpClient _client;

    public RemoteImagesPredictor(
        ILogger<RemoteImagesPredictor> logger,
        IPredictor predictor,
        HttpClient client)
    {
        _logger = logger;
        _predictor = predictor;
        _client = client;
    }
    
    public async Task<IDictionary<string, PredictionResult>> PredictAsync(
        IEnumerable<string> urls,
        CancellationToken ct = default)
    {
        var imagePredictions = new Dictionary<string, PredictionResult>();
        
        var urlsArray = urls.ToArray();
        
        foreach (var urlToPredict in urlsArray)
        {
            _logger.LogInformation("Start loading image [{Url}]", urlToPredict);

            await using var stream = await _client.GetStreamAsync(urlToPredict, ct);
            
            var bytes = ResizeImage(stream, 500, 400);
            
            _logger.LogInformation("Image resized to [{Size}] bytes", bytes.Length);
            
            var base64String = Convert.ToBase64String(bytes);
            
            _logger.LogInformation("Start prediction for image [{Url}]", urlToPredict);
            
            var result = await _predictor.PredictAsync(base64String, ct);
            
            _logger.LogInformation(
                "Prediction finished. Result is [{Result}]",
                result);
            
            imagePredictions.Add(urlToPredict, new PredictionResult
            {
                RenovationRating = result.RenovationRating,
                Description = result.Description,
                Tags = result.Tags
            });
            
            _logger.LogInformation("Completed [{Current}/{Total}]", imagePredictions.Count, urlsArray.Length);
        }

        return imagePredictions;
    }

    private byte[] ResizeImage(Stream imageBytes, int targetWidth, int targetHeight)
    {
        using var originalBitmap = SKBitmap.Decode(imageBytes);
        
        var ratioX = (float)targetWidth / originalBitmap.Width;
        var ratioY = (float)targetHeight / originalBitmap.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(originalBitmap.Width * ratio);
        var newHeight = (int)(originalBitmap.Height * ratio);
        
        using var resizedBitmap = new SKBitmap(newWidth, newHeight);
        
        using var canvas = new SKCanvas(resizedBitmap);
        canvas.Clear();
        using var paint = new SKPaint { FilterQuality = SKFilterQuality.High };
        canvas.DrawBitmap(originalBitmap, new SKRect(0, 0, newWidth, newHeight), paint);
        
        using var outputStream = new MemoryStream();
        resizedBitmap.Encode(outputStream, SKEncodedImageFormat.Jpeg, 30);
        
        outputStream.Seek(0, SeekOrigin.Begin);
        return outputStream.ToArray();
    }
}