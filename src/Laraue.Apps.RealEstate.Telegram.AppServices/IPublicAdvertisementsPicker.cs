using Laraue.Apps.RealEstate.Contracts;

namespace Laraue.Apps.RealEstate.Telegram.AppServices;

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