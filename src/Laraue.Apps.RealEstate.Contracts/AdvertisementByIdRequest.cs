namespace Laraue.Apps.RealEstate.Contracts;

public sealed record AdvertisementByIdRequest
{
    public required long Id { get; init; }
}