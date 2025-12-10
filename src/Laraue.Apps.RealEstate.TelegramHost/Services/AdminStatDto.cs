using Laraue.Apps.RealEstate.Contracts;

namespace Laraue.Apps.RealEstate.TelegramHost.Services;

public sealed record AdminStatDto(
    int UsersCount,
    int ActiveSelectionsCount,
    IList<CrawlingSessionDto> LastCrawlingSessions,
    IList<DailyCrawlingStatDto> DailyCrawlingStats);

public sealed record DailyCrawlingStatDto(
    AdvertisementSource Source,
    int AdvertisementsCount);

public sealed record CrawlingSessionDto(
    long Id,
    DateTime StartedAt,
    DateTime FinishedAt,
    AdvertisementSource Source,
    int AdvertisementsCount);