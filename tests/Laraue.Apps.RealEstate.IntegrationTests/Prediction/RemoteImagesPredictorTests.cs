using System.Text;
using Laraue.Apps.RealEstate.Prediction.Impl;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Laraue.Apps.RealEstate.IntegrationTests.Prediction;

public class RemoteImagesPredictorTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly RemoteImagesPredictor _predictor;

    public RemoteImagesPredictorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _predictor = new RemoteImagesPredictor(
            new Mock<ILogger<RemoteImagesPredictor>>().Object,
            new OllamaRealEstatePredictor(
                new OllamaPredictor(
                    new HttpClient { BaseAddress = new Uri("http://localhost:11434/") },
                    new Mock<ILogger<OllamaPredictor>>().Object),
                new Mock<ILogger<OllamaRealEstatePredictor>>().Object),
            new HttpClient());
    }

    [InlineData("https://images.cdn-cian.ru/images/2615404113-4.jpg", 0, 0)]
    [InlineData("https://images.cdn-cian.ru/images/2612468484-1.jpg", 0.15, 0.45)]
    [InlineData("https://images.cdn-cian.ru/images/2613149942-4.jpg", 0.5, 0.7)]
    [InlineData("https://images.cdn-cian.ru/images/2455996934-1.jpg", 0.75, 1)]
    [InlineData("https://images.cdn-cian.ru/images/2367886657-1.jpg", 0.65, 0.9)]
    [InlineData("https://images.cdn-cian.ru/images/dolya-v-kvartire-sanktpeterburg-alleya-kotelnikova-2609865031-1.jpg", 0.25, 0.5)]
    [Theory]
    public async Task PredictTest(string url, double minRate, double maxRate)
    {
        var result = await _predictor.PredictAsync([url]);
        
        var pair = Assert.Single(result);
        
        Assert.Equal(url, pair.Key);

        var isFailed = false;
        var failMessageBuilder = new StringBuilder();

        void AddFailReason(string reason)
        {
            isFailed = true;
            failMessageBuilder.AppendLine(reason);
        }
        
        if (minRate > pair.Value.RenovationRating)
            AddFailReason($"Excepted rating > {minRate}, got: {pair.Value.RenovationRating}");
        
        if (maxRate < pair.Value.RenovationRating)
            AddFailReason($"Excepted rating < {maxRate}, got: {pair.Value.RenovationRating}");

        if (isFailed)
        {
            failMessageBuilder.AppendLine($"Original tags: {string.Join(',', pair.Value.Tags)}");
            failMessageBuilder.AppendLine($"Original description: {pair.Value.Description}");
        }
        
        Assert.False(isFailed, failMessageBuilder.ToString());
        _testOutputHelper.WriteLine(pair.Value.ToString());
    }
}