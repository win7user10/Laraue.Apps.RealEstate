namespace Laraue.Apps.RealEstate.ApiHost.Requests;

public sealed record GetMainChartRequest
{
    public DateTime? MinDate { get; init; }
    
    public DateTime? MaxDate { get; init; }
}