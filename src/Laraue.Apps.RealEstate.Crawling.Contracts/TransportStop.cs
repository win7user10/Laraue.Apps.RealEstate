using Laraue.Apps.RealEstate.Contracts;

namespace Laraue.Apps.RealEstate.Crawling.Contracts;

/// <summary>
/// Represents a transport stop near the flat.
/// </summary>
public sealed record TransportStop
{
    /// <summary>
    /// Transport stop name.
    /// </summary>
    public string Name { get; init; } = default!;
    
    /// <summary>
    /// How long to get to this stop from the apartments.
    /// </summary>
    public int Minutes { get; init; }
    
    /// <summary>
    /// How the distance to the closest public stop was measured.
    /// </summary>
    public DistanceType DistanceType { get; init; }
}