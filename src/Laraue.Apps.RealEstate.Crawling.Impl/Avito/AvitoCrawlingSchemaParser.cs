using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.Avito;
using Laraue.Crawling.Dynamic.PuppeterSharp.Abstractions;
using Laraue.Crawling.Dynamic.PuppeterSharp.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace Laraue.Apps.RealEstate.Crawling.Impl.Avito;

public sealed class AvitoCrawlingSchemaParser : BaseCrawlingSchemaParser, IAvitoCrawlingSchemaParser
{
    public AvitoCrawlingSchemaParser(
        ILogger<BaseCrawlingSchemaParser> logger,
        IPageParser pageParser,
        IAvitoCrawlingSchema crawlingSchema,
        IOptions<AvitoCrawlerServiceOptions> options,
        IBrowserFactory browserFactory)
            : base(logger, pageParser, crawlingSchema, options, browserFactory)
    {
    }
    
    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36";

    protected override Task SetupBrowserPageAsync(IPage page)
    {
        return page.SetExtraHttpHeadersAsync(new Dictionary<string, string>()
        {
            ["Accept-Language"] = "en,en-US;q=0.9,ru-RU;q=0.8,ru;q=0.7",
            ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7",
            ["User-Agent"] = UserAgent,
        });
    }
}