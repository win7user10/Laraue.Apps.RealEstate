namespace Laraue.Apps.RealEstate.DataAccess.Models;

public class AdvertisementImage
{
    /// <summary>
    /// Relation to the advertisement.
    /// </summary>
    public Advertisement Advertisement { get; init; } = default!;
    
    /// <summary>
    /// Advertisement identifier.
    /// </summary>
    public long AdvertisementId { get; init; }
    
    /// <summary>
    /// Relation to the advertisement.
    /// </summary>
    public Image Image { get; init; } = default!;
    
    /// <summary>
    /// Advertisement identifier.
    /// </summary>
    public long ImageId { get; init; }
}