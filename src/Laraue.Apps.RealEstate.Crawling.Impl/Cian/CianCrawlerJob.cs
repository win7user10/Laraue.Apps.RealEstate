using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.Cian;
using Laraue.Apps.RealEstate.Db;
using Laraue.Core.DateTime.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Laraue.Apps.RealEstate.Crawling.Impl.Cian;

public sealed class CianCrawlerJob : BaseRealEstateCrawlerJob
{
    public CianCrawlerJob(
        ILogger<CianCrawlerJob> logger,
        IOptions<CianCrawlerServiceOptions> options,
        IDateTimeProvider dateTimeProvider,
        AdvertisementsDbContext dbContext,
        ICianAdvertisementProcessor processor,
        ICianCrawlingSchemaParser parser)
            : base(logger, options, dateTimeProvider, dbContext, processor, parser)
    {
    }

    protected override string AdvertisementsAddress
        => "https://spb.cian.ru/cat.php?deal_type=sale&engine_version=2&offer_type=flat&p={0}&region=2&sort=creation_date_desc";
}