namespace Laraue.Apps.RealEstate.Db.Models;

public record TransportStop
{
    public long Id { get; init; }

    public string Name { get; init; } = default!;

    public string Color { get; init; } = default!;
    
    public byte Priority { get; init; }
}