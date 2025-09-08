using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.Cian;
using Laraue.Crawling.Dynamic.PuppeterSharp.Abstractions;
using Laraue.Crawling.Dynamic.PuppeterSharp.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace Laraue.Apps.RealEstate.Crawling.Impl.Cian;

public sealed class CianCrawlingSchemaParser : BaseCrawlingSchemaParser, ICianCrawlingSchemaParser
{
    public CianCrawlingSchemaParser(
        ILogger<BaseCrawlingSchemaParser> logger,
        IPageParser pageParser,
        ICianCrawlingSchema crawlingSchema,
        IOptions<CianCrawlerServiceOptions> options,
        IBrowserFactory browserFactory)
            : base(logger, pageParser, crawlingSchema, options, browserFactory)
    {
    }

    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
        "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 Safari/537.36";

    protected override Task SetupBrowserPageAsync(IPage page)
    {
        return page.SetUserAgentAsync(UserAgent);
    }
}