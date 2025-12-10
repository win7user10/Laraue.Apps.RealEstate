namespace Laraue.Apps.RealEstate.DataAccess.Models;

public record TransportStop
{
    public long Id { get; init; }

    public string Name { get; init; } = default!;

    public string Color { get; init; } = default!;
    
    public byte Priority { get; init; }
}