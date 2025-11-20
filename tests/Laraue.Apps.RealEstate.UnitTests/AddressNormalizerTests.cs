using Laraue.Apps.RealEstate.Crawling.Impl;

namespace Laraue.Apps.RealEstate.UnitTests;

public class AddressNormalizerTests
{
    [Theory]
    [InlineData("3-4А21", "3-4а21")]
    public void NormalizeHouseNumber_Always_Success(string actual, string excepted)
    {
        var normalized = AddressNormalizer.NormalizeHouseNumber(actual);
        
        Assert.Equal(excepted, normalized);
    }
    
    [Theory]
    [InlineData("123-125", "123,125")]
    public void SplitHouseNumber_Always_Success(string actual, string excepted)
    {
        var normalized = AddressNormalizer.SplitHouseNumber(actual);
        
        var exceptedArray = excepted.Split(',');
        
        Assert.Equal(exceptedArray, normalized);
    }
    
    [Theory]
    [InlineData("Пр-т Ветеранов", "проспект Ветеранов")]
    [InlineData("б-р Победы", "бульвар Победы")]
    [InlineData("ул. Кирова", "улица Кирова")]
    [InlineData("наб. реки Фонтанки", "набережная реки Фонтанки")]
    public void NormalizeStreet_Always_Success(string actual, string excepted)
    {
        var normalized = AddressNormalizer.NormalizeStreet(actual);
        
        Assert.Equal(excepted, normalized);
    }
}