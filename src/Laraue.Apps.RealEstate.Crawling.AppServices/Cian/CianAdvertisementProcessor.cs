using Laraue.Apps.RealEstate.AppServices.TransportStops;
using Laraue.Apps.RealEstate.Contracts;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.DataAccess.Storage;
using Laraue.Apps.RealEstate.Db;
using Laraue.Core.DateTime.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Laraue.Apps.RealEstate.Crawling.AppServices.Cian;

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