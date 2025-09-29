namespace Laraue.Apps.RealEstate.Db.Models;

public sealed record Image
{
    public long Id { get; set; }
    
    /// <summary>
    /// Rating of renovation if it exists.
    /// </summary>
    public double RenovationRating { get; set; }

    /// <summary>
    /// Image address.
    /// </summary>
    public string Url { get; init; } = default!;

    public string? Description { get; set; }
    public string[] Tags { get; set; } = [];
    
    public DateTime? PredictedAt { get; set; }
}