using Laraue.Apps.RealEstate.Crawling.Impl.Avito;
using Laraue.Core.DateTime.Services.Abstractions;
using Moq;
using Xunit;

namespace Laraue.Apps.RealEstate.IntegrationTests.Crawling.Avito;

public class AvitoDateParserTests
{
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();

    public AvitoDateParserTests()
    {
        _dateTimeProvider.Setup(x => x.UtcNow)
            .Returns(new DateTime(2020, 12, 30, 22, 50, 00, DateTimeKind.Utc));
    }

    [Theory]
    [InlineData("10 августа 13:46", 10, 08, 13, 46)]
    [InlineData("28 дней назад", 2, 12, 0, 0)]
    [InlineData("3 дня назад", 27, 12, 0, 0)]
    [InlineData("1 день назад", 29, 12, 0, 0)]
    [InlineData("Несколько секунд назад", 30, 12, 22, 50)]
    [InlineData("1 минуту назад", 30, 12, 22, 49)]
    [InlineData("2 минуты назад", 30, 12, 22, 48)]
    [InlineData("5 минут назад", 30, 12, 22, 45)]
    [InlineData("1 час назад", 30, 12, 21, 00)]
    [InlineData("2 часа назад", 30, 12, 20, 00)]
    [InlineData("5 часов назад", 30, 12, 17, 00)]
    public void Extract_ShouldBeMadeCorrectly(string source, int day, int month, int hour, int minute)
    {
        var result = AvitoDateParser.Parse(source, _dateTimeProvider.Object);
        
        Assert.Equal(new DateTime(_dateTimeProvider.Object.UtcNow.Year, month, day, hour, minute, 0, DateTimeKind.Utc), result);
    }
}