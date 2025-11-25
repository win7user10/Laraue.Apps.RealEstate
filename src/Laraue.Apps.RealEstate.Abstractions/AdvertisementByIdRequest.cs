namespace Laraue.Apps.RealEstate.Abstractions;

public sealed record AdvertisementByIdRequest
{
    public required long Id { get; init; }
}