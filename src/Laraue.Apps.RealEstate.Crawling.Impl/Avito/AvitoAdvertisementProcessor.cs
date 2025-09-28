using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.Avito;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.TransportStops;
using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.Prediction.Abstractions;

namespace Laraue.Apps.RealEstate.Crawling.Impl.Avito;

public sealed class AvitoAdvertisementProcessor : BaseAdvertisementProcessor<long>, IAvitoAdvertisementProcessor
{
    public AvitoAdvertisementProcessor(
        AdvertisementsDbContext dbContext,
        IAverageRatingCalculator calculator,
        IMetroStationsStorage metroStationsStorage,
        IAdvertisementComputedFieldsCalculator computedFieldsCalculator)
        : base(
            AdvertisementSource.Avito,
            dbContext,
            calculator,
            computedFieldsCalculator,
            metroStationsStorage)
    {
    }

    protected override long ParseIdentifier(string stringIdentifier)
    {
        return long.Parse(stringIdentifier);
    }
}