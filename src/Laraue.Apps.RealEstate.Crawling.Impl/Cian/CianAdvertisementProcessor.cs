using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.Cian;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.TransportStops;
using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.Prediction.Abstractions;

namespace Laraue.Apps.RealEstate.Crawling.Impl.Cian;

public sealed class CianAdvertisementProcessor : BaseAdvertisementProcessor<int>, ICianAdvertisementProcessor
{
    public CianAdvertisementProcessor(
        AdvertisementsDbContext dbContext,
        IRemoteImagesPredictor imagesPredictor,
        IAverageRatingCalculator calculator,
        IMetroStationsStorage metroStationsStorage,
        IAdvertisementComputedFieldsCalculator computedFieldsCalculator)
        : base(
            AdvertisementSource.Cian,
            dbContext,
            imagesPredictor,
            calculator,
            computedFieldsCalculator,
            metroStationsStorage)
    {
    }

    protected override int ParseIdentifier(string stringIdentifier)
    {
        return int.Parse(stringIdentifier);
    }
}