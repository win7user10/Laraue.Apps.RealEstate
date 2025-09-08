using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;
using Laraue.Apps.RealEstate.Crawling.Impl.Cian;
using Xunit;

namespace Laraue.Apps.RealEstate.IntegrationTests.Crawling.Cian;

public class CianTitleExtractorTests
{
    [Theory]
    [InlineData("1-комн. кв., 44 м², 14/17 этаж", 1, 44.0, 14, 17, FlatType.Flat)]
    [InlineData("2-комн. кв., 53,3 м², 5/18 этаж", 2, 53.3, 5, 18, FlatType.Flat)]
    [InlineData("2-комн. квартира, 65,7 м², 3/21 этаж", 2, 65.7, 3, 21, FlatType.Flat)]
    [InlineData("2-комнатная квартира площадью 63.81 кв.м.", 2, 63.81, null, null, FlatType.Flat)]
    [InlineData("2-комн. апартаменты, 65,56 м², 7/10 этаж", 2, 65.56, 7, 10, FlatType.Apartments)]
    [InlineData("Студия, 10 м², 3/7 этаж", null, 10, 3, 7, FlatType.Flat)]
    [InlineData("Апартаменты-студия, 22.2 м², 14/20 этаж", null, 22.2, 14, 20, FlatType.Apartments)]
    public void Extract_ShouldBeMadeCorrectly(
        string source,
        int? roomsNumber,
        double square,
        int? floor,
        int? totalFloors,
        FlatType flatType)
    {
        var result = CianTitleExtractor.ExtractTitle(source, null);
        
        Assert.Equal(roomsNumber, result.RoomsNumber);
        Assert.Equal((decimal)square, result.Square);
        Assert.Equal(floor, result.Floor);
        Assert.Equal(totalFloors, result.TotalFloors);
        Assert.Equal(flatType, result.FlatType);
    }
}