using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.Cian;
using Laraue.Apps.RealEstate.Db;
using Laraue.Core.DateTime.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Laraue.Apps.RealEstate.Crawling.Impl.Cian;

[JobGroup("CianCrawlerJob")]
public class CianCrawlerJob : BaseRealEstateCrawlerJob
{
    protected CianCrawlerJob(
        ILogger<CianCrawlerJob> logger,
        IOptions<CianCrawlerServiceOptions> options,
        IDateTimeProvider dateTimeProvider,
        AdvertisementsDbContext dbContext,
        ICianAdvertisementProcessor processor,
        ICianCrawlingSchemaParser parser,
        long cityId,
        string advertisementsAddress)
            : base(logger, options, dateTimeProvider, dbContext, processor, parser)
    {
        CityId = cityId;
        AdvertisementsAddress = advertisementsAddress;
    }

    protected override string AdvertisementsAddress { get; }

    protected override long CityId { get; }
}