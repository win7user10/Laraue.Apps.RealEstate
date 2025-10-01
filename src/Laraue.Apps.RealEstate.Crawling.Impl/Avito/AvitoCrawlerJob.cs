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
        => "https://www.avito.ru/sankt-peterburg/kvartiry/prodam/vtorichka-ASgBAgICAkSSA8YQ5geMUg?cd=1&context=H4sIAAAAAAAA_wEjANz_YToxOntzOjg6ImZyb21QYWdlIjtzOjc6ImNhdGFsb2ciO312FITcIwAAAA&f=ASgBAQICAkSSA8YQ5geMUgFAkL4NJJSuNZauNQ&localPriority=0&s=104&p={0}";
}