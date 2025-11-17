using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Db;
using Laraue.Core.DataAccess.Contracts;
using Laraue.Core.DateTime.Services.Abstractions;
using LinqToDB;

namespace Laraue.Apps.RealEstate.Telegram;

public sealed class PublicAdvertisementsPicker : IPublicAdvertisementsPicker
{
    private readonly AdvertisementsDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAdvertisementStorage _advertisementStorage;

    public PublicAdvertisementsPicker(
        AdvertisementsDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        IAdvertisementStorage advertisementStorage)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _advertisementStorage = advertisementStorage;
    }

    public async Task<BestAdvertisementsGroupResponse> GetBestSinceSessionAsync(
        long? previousSessionId,
        CancellationToken cancellationToken = default)
    {
        var minValue = DateTime.MinValue;
        if (previousSessionId is not null)
        {
            var previousSession = await _dbContext.CrawlingSessions
                .Where(x => x.Id == previousSessionId)
                .FirstAsync(cancellationToken);

            minValue = previousSession.FinishedAt;
        }
        
        var lastSession = await _dbContext.CrawlingSessions
            .OrderByDescending(x => x.FinishedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var maxDate = lastSession?.FinishedAt ?? _dateTimeProvider.UtcNow;
        
        var request = new AdvertisementsRequest
        {
            Filter = new Filter
            {
                MaxDate = maxDate,
                MinDate = minValue,
                MinRenovationRating = 7,
                SortBy = AdvertisementsSort.RealSquareMeterPrice,
                SortOrderBy = SortOrder.Ascending,
                MinPrice = 5_000_000,
                MaxPrice = 8_999_999,
            },
            Pagination = new PaginationData
            {
                PerPage = 3,
            }
        };

        var advertisements = await _advertisementStorage
            .GetAdvertisementsAsync(request);

        return new BestAdvertisementsGroupResponse
        {
            Advertisements = advertisements.Data,
            LastSessionId = lastSession?.Id
        };
    }
}