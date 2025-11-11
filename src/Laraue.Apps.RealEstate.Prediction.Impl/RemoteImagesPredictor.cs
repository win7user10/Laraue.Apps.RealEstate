using System.Collections.Concurrent;
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
        var imagePredictions = new ConcurrentDictionary<string, PredictionResult>();
        
        var urlsArray = urls.ToArray();

        var options = new ParallelOptions { MaxDegreeOfParallelism = 2, CancellationToken = ct };
        await Parallel.ForEachAsync(urlsArray, options, async (url, innerCt) =>
        {
            innerCt.ThrowIfCancellationRequested();

            _logger.LogInformation("Start loading image [{Url}]", url);

            var result = await PredictAsync(url, innerCt);
            imagePredictions.TryAdd(url, result);

            _logger.LogInformation("Completed [{Current}/{Total}]", imagePredictions.Count, urlsArray.Length);
        });

        return imagePredictions;
    }

    private async Task<PredictionResult> PredictAsync(
        string urlToPredict,
        CancellationToken ct)
    {
        try
        {
            var bytes = await _client.GetByteArrayAsync(urlToPredict, ct);

            // var bytes = ResizeImage(stream, 500, 400);

            _logger.LogInformation("Image Size is [{Size}] bytes", bytes.Length);

            var base64String = Convert.ToBase64String(bytes);

            _logger.LogDebug("Start prediction for image [{Url}]", urlToPredict);

            var result = await _predictor.PredictAsync(base64String, ct);

            _logger.LogDebug(
                "Prediction finished. Result is [{Result}]",
                result);

            var renovationRating = result.RenovationRating;
            
            // Sometimes model hallucinates and setas a score for unrelated image.
            if (result.Tags.Any(x => x.Contains("exterior", StringComparison.InvariantCultureIgnoreCase)))
            {
                renovationRating = 0;
            }

            return new PredictionResult
            {
                RenovationRating = renovationRating,
                Description = result.Description,
                Tags = result.Tags
            };
        }
        catch (Exception)
        {
            return new PredictionResult
            {
                ErrorWhileRequesting = true
            };
        }
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