namespace Laraue.Apps.RealEstate.Crawling.Contracts.Contracts;

public record FlatAddress
{
    public required string Street { get; init; }
    public required string HouseNumber { get; init; }
}