using Laraue.Apps.RealEstate.Abstractions;

namespace Laraue.Apps.RealEstate.Telegram;

public interface IPublicAdvertisementsPicker
{
    Task<BestAdvertisementsGroupResponse> GetBestSinceSessionAsync(
        long? previousSessionId,
        CancellationToken cancellationToken = default);
}

public record BestAdvertisementsGroupResponse
{
    public required long? LastSessionId { get; init; }
    public required IList<AdvertisementDto> Advertisements { get; init; }
}