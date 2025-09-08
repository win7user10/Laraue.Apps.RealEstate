namespace Laraue.Apps.RealEstate.Abstractions;

public sealed record MainChartDayItemDto
{
    public required DateTime Date { get; init; }

    public IEnumerable<MainChartItemDayItemDataDto> Data { get; init; }

    public double AveragePrice => Data.Average(x => x.AveragePrice);

    public double AverageSquareMeterPrice => Data.Average(x => x.AverageSquareMeterPrice);

    public double AdvertisementCount => Data.Average(x => x.AdvertisementCount);
}

public sealed record MainChartItemDayItemDataDto
{
    public required int AveragePrice { get; init; }
    
    public required int AverageSquareMeterPrice { get; init; }

    public required int AdvertisementCount { get; init; }
    
    public required AdvertisementSource SourceType { get; init; }
}