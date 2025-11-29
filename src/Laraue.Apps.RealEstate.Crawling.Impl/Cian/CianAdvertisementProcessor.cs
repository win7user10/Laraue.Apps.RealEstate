using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.Cian;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.TransportStops;
using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.Db.Storage;
using Laraue.Core.DateTime.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Laraue.Apps.RealEstate.Crawling.Impl.Cian;

public sealed class CianAdvertisementProcessor : BaseAdvertisementProcessor<int>, ICianAdvertisementProcessor
{
    public CianAdvertisementProcessor(
        AdvertisementsDbContext dbContext,
        IMetroStationsStorage metroStationsStorage,
        IDateTimeProvider dateTimeProvider,
        ILogger<CianAdvertisementProcessor> logger,
        IHousesStorage housesStorage)
        : base(
            AdvertisementSource.Cian,
            dbContext,
            metroStationsStorage,
            dateTimeProvider,
            logger,
            housesStorage)
    {
    }

    protected override int ParseIdentifier(string stringIdentifier)
    {
        return int.Parse(stringIdentifier);
    }
}