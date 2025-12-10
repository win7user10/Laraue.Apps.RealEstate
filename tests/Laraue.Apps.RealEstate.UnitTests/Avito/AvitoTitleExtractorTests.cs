using Laraue.Apps.RealEstate.Crawling.AppServices.Avito;
using Laraue.Apps.RealEstate.Crawling.Contracts;

namespace Laraue.Apps.RealEstate.UnitTests.Avito;

public class AvitoTitleExtractorTests
{
    [Theory]
    [InlineData("Квартира-студия, 28 м², 5/16 эт.", null, 28, 5, 16, FlatType.Flat)]
    [InlineData("2-к. квартира, 61,1 м², 4/11 эт.", 2, 61.1, 4, 11, FlatType.Flat)]
    [InlineData("1-к. апартаменты, 37 м², 11/13 эт.", 1, 37, 11, 13, FlatType.Apartments)]
    [InlineData("Апартаменты-студия, 34 м², 5/12 эт.", null, 34, 5, 12, FlatType.Apartments)]
    [InlineData("6-к. квартира, 212 м², 6/6 эт.", 6, 212, 6, 6, FlatType.Flat)]
    [InlineData("Доля в 1-к. квартире, 45 м², 2/4 эт.", 1, 45, 2, 4, FlatType.Flat)]
    [InlineData("3-к. квартира, 77,7 м², 1/5 эт.", 3, 77.7, 1, 5, FlatType.Flat)]
    public void Extract_ShouldBeMadeCorrectly(
        string source,
        int? roomsNumber,
        double square,
        int? floor,
        int? totalFloors,
        FlatType flatType)
    {
        var result = AvitoTitleExtractor.Extract(source);
        
        Assert.Equal(roomsNumber, result.RoomsNumber);
        Assert.Equal((decimal)square, result.Square);
        Assert.Equal(floor, result.Floor);
        Assert.Equal(totalFloors, result.TotalFloors);
        Assert.Equal(flatType, result.FlatType);
    }
}