namespace Laraue.Apps.RealEstate.Abstractions;

public struct DateInterval
{
    public required DateTime From { get; init; }
    
    public required DateTime To { get; init; }
}