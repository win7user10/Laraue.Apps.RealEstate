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
        IAvitoCrawlingSchemaParser parser,
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