namespace Laraue.Apps.RealEstate.Telegram;

public sealed record AdvertisementsSenderOptions
{
    public required string Token { get; init; }
    
    public required string ChatId { get; init; }
}