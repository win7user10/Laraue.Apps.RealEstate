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
                    new Mock<ILogger<OllamaPredictor>>().Object)),
            new HttpClient());
    }

    [InlineData("https://images.cdn-cian.ru/images/2615404113-4.jpg", 0, 0)]
    [InlineData("https://images.cdn-cian.ru/images/2612468484-1.jpg", 0.15, 0.45)]
    [InlineData("https://images.cdn-cian.ru/images/2613149942-4.jpg", 0.5, 0.7)]
    [InlineData("https://images.cdn-cian.ru/images/2455996934-1.jpg", 0.75, 1)]
    [InlineData("https://images.cdn-cian.ru/images/2367886657-1.jpg", 0.65, 0.9)]
    [InlineData("https://images.cdn-cian.ru/images/dolya-v-kvartire-sanktpeterburg-alleya-kotelnikova-2609865031-1.jpg", 0.25, 0.5)]
    [InlineData("https://images.cdn-cian.ru/images/2623741669-4.jpg", 0, 0)]
    [Theory]
    public async Task PredictTest(string url, double minRate, double maxRate)
    {
        var result = await _predictor.PredictAsync([url]);

        var isFailed = false;
        var failMessageBuilder = new StringBuilder();

        void AddFailReason(string reason)
        {
            isFailed = true;
            failMessageBuilder.AppendLine(reason);
        }
        
        if (minRate > result.RenovationRating)
            AddFailReason($"Excepted rating > {minRate}, got: {result.RenovationRating}");
        
        if (maxRate < result.RenovationRating)
            AddFailReason($"Excepted rating < {maxRate}, got: {result.RenovationRating}");

        if (isFailed)
        {
            failMessageBuilder.AppendLine($"Advantages: {string.Join(',', result.Advantages)}");
            failMessageBuilder.AppendLine($"Problems: {string.Join(',', result.Problems)}");
        }
        
        Assert.False(isFailed, failMessageBuilder.ToString());
    }
}