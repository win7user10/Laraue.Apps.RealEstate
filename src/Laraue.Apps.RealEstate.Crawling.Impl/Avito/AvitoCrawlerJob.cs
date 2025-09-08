using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.Avito;
using Laraue.Apps.RealEstate.Db;
using Laraue.Core.DateTime.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Laraue.Apps.RealEstate.Crawling.Impl.Avito;

public sealed class AvitoCrawlerJob : BaseRealEstateCrawlerJob
{
    public AvitoCrawlerJob(
        ILogger<AvitoCrawlerJob> logger,
        IOptions<AvitoCrawlerServiceOptions> options,
        IDateTimeProvider dateTimeProvider,
        AdvertisementsDbContext dbContext,
        IAvitoAdvertisementProcessor processor,
        IAvitoCrawlingSchemaParser parser)
            : base(logger, options, dateTimeProvider, dbContext, processor, parser)
    {
    }

    protected override string AdvertisementsAddress
        => "https://www.avito.ru/sankt-peterburg/kvartiry/prodam-ASgBAgICAUSSA8YQ?cd=1&p={0}&s=104";
}