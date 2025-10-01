using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.Avito;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.TransportStops;
using Laraue.Apps.RealEstate.Db;
using Laraue.Core.DateTime.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Laraue.Apps.RealEstate.Crawling.Impl.Avito;

public sealed class AvitoAdvertisementProcessor : BaseAdvertisementProcessor<long>, IAvitoAdvertisementProcessor
{
    public AvitoAdvertisementProcessor(
        AdvertisementsDbContext dbContext,
        IMetroStationsStorage metroStationsStorage,
        IDateTimeProvider dateTimeProvider,
        ILogger<AvitoAdvertisementProcessor> logger)
        : base(
            AdvertisementSource.Avito,
            dbContext,
            metroStationsStorage,
            dateTimeProvider,
            logger)
    {
    }

    protected override long ParseIdentifier(string stringIdentifier)
    {
        return long.Parse(stringIdentifier);
    }
}