namespace Laraue.Apps.RealEstate.Abstractions;

public sealed record RangeChartRequest
{
    public DateTime? MinDate { get; init; }
    
    public DateTime? MaxDate { get; init; }
}