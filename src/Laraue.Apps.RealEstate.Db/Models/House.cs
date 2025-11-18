using System.ComponentModel.DataAnnotations;

namespace Laraue.Apps.RealEstate.Db.Models;

public class House
{
    public long StreetId { get; init; }

    public Street Street { get; init; } = null!;

    /// <summary>
    /// Contain the dull house number string, can be 183-185Ак2А, for example.
    /// </summary>
    [MaxLength(200)]
    public required string Address { get; init; }
    
    /// <summary>
    /// Normalized house number to make it searchable, e.g., [183, 184, 185Ак2А]
    /// </summary>
    public required string[] AddressSegmentsNormalized { get; init; }
}