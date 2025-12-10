namespace Laraue.Apps.RealEstate.Contracts;

public struct DateInterval
{
    public required DateTime From { get; init; }
    
    public required DateTime To { get; init; }
}