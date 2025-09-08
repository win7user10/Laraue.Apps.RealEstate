using Laraue.Apps.RealEstate.Crawling.Impl.Avito;
using Xunit;

namespace Laraue.Apps.RealEstate.IntegrationTests.Crawling.Avito;

public sealed class AvitoTimeParserTests
{
    [Theory]
    [InlineData("11-15 мин.", 15)]
    [InlineData("6-10 мин.", 10)]
    [InlineData("от 31 мин.", 31)]
    [InlineData("до 5 мин.", 5)]
    public void Extract_ShouldBeMadeCorrectly(string source, int excepted)
    {
        var result = AvitoTimeParser.Parse(source);
        
        Assert.Equal(excepted, result);
    }
}