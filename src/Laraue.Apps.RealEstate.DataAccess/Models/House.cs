using System.ComponentModel.DataAnnotations;

namespace Laraue.Apps.RealEstate.DataAccess.Models;

public class House
{
    public long Id { get; init; }
    
    public long StreetId { get; init; }

    public Street Street { get; init; } = null!;

    /// <summary>
    /// Contain the dull house number string, can be 183-185Ак2А, for example.
    /// </summary>
    [MaxLength(100)]
    public required string Number { get; init; }
    
    /// <summary>
    /// Normalized house number to make it searchable, e.g., [183, 184, 185]
    /// </summary>
    public required string[] NumberNormalized { get; set; }
}