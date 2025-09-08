using Laraue.Apps.RealEstate.Prediction.Abstractions;
using Laraue.Apps.RealEstate.Prediction.Impl;
using Xunit;

namespace Laraue.Apps.RealEstate.IntegrationTests.Prediction;

public class AverageRatingCalculatorTests
{
    private readonly AverageRatingCalculator _calculator;

    public AverageRatingCalculatorTests()
    {
        _calculator = new AverageRatingCalculator();
    }

    [Fact]
    public void AverageRating_ShouldReturn_WhenRenovatedItemsAreEnough()
    {
        var result = _calculator.Calculate(new PredictionResult[]
        {
            new () { RenovationRating = 0.65 },
            new () { RenovationRating = 0.72 },
            new (),
            new (),
        });
        
        Assert.Equal(0.685, result);
    }
    
    [Fact]
    public void Rating_ShouldNotReturn_WhenRenovatedItemsAreNotEnough()
    {
        var result = _calculator.Calculate([
            new () { RenovationRating = 0.65 },
            new ()
        ]);
        
        Assert.Null(result);
    }
    
    [Fact]
    public void Renovation_ShouldBeFalse_WhenLessThan66PercentOfImagesWithRating()
    {
        var result = _calculator.Calculate([
            new () { RenovationRating = 0.65 },
            new () { RenovationRating = 0.73 },
            new (),
            new (),
            new ()
        ]);
        
        Assert.Null(result);
    }
}