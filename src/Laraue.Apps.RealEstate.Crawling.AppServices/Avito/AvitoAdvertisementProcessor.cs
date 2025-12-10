using Laraue.Apps.RealEstate.AppServices.TransportStops;
using Laraue.Apps.RealEstate.Contracts;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.DataAccess.Storage;
using Laraue.Apps.RealEstate.Db;
using Laraue.Core.DateTime.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Laraue.Apps.RealEstate.Crawling.AppServices.Avito;

public sealed class AvitoAdvertisementProcessor : BaseAdvertisementProcessor<long>, IAvitoAdvertisementProcessor
{
    public AvitoAdvertisementProcessor(
        AdvertisementsDbContext dbContext,
        IMetroStationsStorage metroStationsStorage,
        IDateTimeProvider dateTimeProvider,
        ILogger<AvitoAdvertisementProcessor> logger,
        IHousesStorage housesStorage)
        : base(
            AdvertisementSource.Avito,
            dbContext,
            metroStationsStorage,
            dateTimeProvider,
            logger,
            housesStorage)
    {
    }

    protected override long ParseIdentifier(string stringIdentifier)
    {
        return long.Parse(stringIdentifier);
    }
}