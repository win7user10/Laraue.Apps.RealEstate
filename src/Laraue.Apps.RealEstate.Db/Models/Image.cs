namespace Laraue.Apps.RealEstate.Db.Models;

public sealed record Image
{
    public long Id { get; set; }
    
    /// <summary>
    /// Rating of renovation if it exists.
    /// </summary>
    public double RenovationRating { get; init; }

    /// <summary>
    /// Image address.
    /// </summary>
    public string Url { get; init; } = default!;

    public string? Description { get; init; }
    public string[] Tags { get; init; } = [];
    
    public AdvertisementImageProcessState ProcessState { get; init; }
}

public enum AdvertisementImageProcessState
{
    None,
    Processed,
}