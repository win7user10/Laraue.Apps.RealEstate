namespace Laraue.Apps.RealEstate.Contracts;

public sealed record RangeChartRequest
{
    public DateTime? MinDate { get; init; }
    
    public DateTime? MaxDate { get; init; }
}