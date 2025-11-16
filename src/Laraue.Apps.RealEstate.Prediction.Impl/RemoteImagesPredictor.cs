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
    
    public async Task<PredictionResult> PredictAsync(
        IEnumerable<string> urls,
        CancellationToken ct = default)
    {
        var images = new List<byte[]>();

        foreach (var url in urls)
        {
            try
            {
                images.Add(await _client.GetByteArrayAsync(url, ct));
            }
            catch (Exception)
            {
                _logger.LogDebug("Failed loading [{Url}]", url);
            }
        }

        // It is not enough images too ranking
        if (images.Count == 0)
        {
            return new PredictionResult
            {
                RenovationRating = 0
            };
        }
        
        var mergedImage = MergeImages(images);
        _logger.LogInformation("Merged image size is {Size} MB", mergedImage.Length / 1024 / 1024);
        var base64String = Convert.ToBase64String(mergedImage);

        var predictionResult = await _predictor.PredictAsync(base64String, ct);
        return new PredictionResult
        {
            Advantages = predictionResult.Advantages,
            Problems = predictionResult.Problems,
            RenovationRating = predictionResult.RenovationRating,
        };
    }

    private static byte[] MergeImages(IEnumerable<byte[]> images)
    {
        var bitmaps = images.Select(SKBitmap.Decode);

        var merged = MergeImagesWithBorders(bitmaps);
        
        using var data = merged.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
    
    private static SKImage MergeImagesWithBorders(
        IEnumerable<SKBitmap> images,
        int borderWidth = 2,
        SKColor borderColor = default)
    {
        if (borderColor == default) borderColor = SKColors.Black;
    
        var imageList = images.ToList();
    
        // Calculate total dimensions
        var totalWidth = imageList.Sum(img => img.Width) + (imageList.Count - 1) * borderWidth;
        var maxHeight = imageList.Max(img => img.Height);
    
        // Create canvas
        using var surface = SKSurface.Create(new SKImageInfo(totalWidth, maxHeight));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
    
        // Draw images with borders
        var currentX = 0;
        foreach (var image in imageList)
        {
            // Draw image
            canvas.DrawBitmap(image, new SKPoint(currentX, 0));
        
            // Draw border (right side only for all but last image)
            if (currentX > 0)
            {
                using var paint = new SKPaint { Color = borderColor, StrokeWidth = borderWidth };
                canvas.DrawLine(currentX, 0, currentX, maxHeight, paint);
            }
        
            currentX += image.Width + borderWidth;
        }
    
        return surface.Snapshot();
    }
}