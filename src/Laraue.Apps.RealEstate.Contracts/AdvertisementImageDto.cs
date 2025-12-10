namespace Laraue.Apps.RealEstate.Contracts;

public record AdvertisementImageDto
{
    public required string Url { get; init; }
    public string? Description { get; init; }
}