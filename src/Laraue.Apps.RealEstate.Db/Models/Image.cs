namespace Laraue.Apps.RealEstate.Db.Models;

public sealed record Image
{
    public long Id { get; set; }
    /// <summary>
    /// Image address.
    /// </summary>
    public string Url { get; init; } = default!;
}