namespace Laraue.Apps.RealEstate.Abstractions;

public record AdvertisementImageDto
{
    public required string Url { get; init; }
    public double RenovationRating { get; init; }
    public string[]? Tags { get; init; }
    public string? Description { get; init; }
}