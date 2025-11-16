namespace Laraue.Apps.RealEstate.Db.Models;

public sealed record Image
{
    public long Id { get; set; }
    /// <summary>
    /// Image address.
    /// </summary>
    public string Url { get; init; } = default!;
    
    /// <summary>
    /// Last 200 response of the link.
    /// </summary>
    public DateTime LastAvailableAt { get; init; } = default!;
}