namespace Laraue.Apps.RealEstate.Db.Models;

public sealed record AdvertisementImage
{
    public long Id { get; set; }
    
    /// <summary>
    /// Rating of renovation if it is exists.
    /// </summary>
    public double RenovationRating { get; init; }

    /// <summary>
    /// Image address.
    /// </summary>
    public string Url { get; init; } = default!;

    /// <summary>
    /// Relation to the advertisement.
    /// </summary>
    public Advertisement Advertisement { get; init; } = default!;
    
    /// <summary>
    /// Advertisement identifier.
    /// </summary>
    public long AdvertisementId { get; init; }

    public string? Decription { get; init; }
    public string[] Tags { get; init; } = [];
}