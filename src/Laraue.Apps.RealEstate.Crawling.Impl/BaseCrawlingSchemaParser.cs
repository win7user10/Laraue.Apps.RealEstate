using System.Net;
using Laraue.Apps.RealEstate.Crawling.Contracts.Contracts;
using Laraue.Apps.RealEstate.Crawling.Contracts.Crawler;
using Laraue.Apps.RealEstate.Crawling.Impl.Exceptions;
using Laraue.Crawling.Crawler;
using Laraue.Crawling.Dynamic.PuppeterSharp.Abstractions;
using Laraue.Crawling.Dynamic.PuppeterSharp.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using PuppeteerSharp;

namespace Laraue.Apps.RealEstate.Crawling.Impl;

public abstract class BaseCrawlingSchemaParser : ICrawlingSchemaParser
{
    private readonly ILogger<BaseCrawlingSchemaParser> _logger;
    private readonly IPageParser _pageParser;
    private readonly ICrawlingSchema _crawlingSchema;
    private readonly IBrowserFactory _browserFactory;
    private readonly BaseCrawlerServiceOptions _options;

    private IPage? _page;

    protected BaseCrawlingSchemaParser(
        ILogger<BaseCrawlingSchemaParser> logger,
        IPageParser pageParser,
        ICrawlingSchema crawlingSchema,
        IOptions<BaseCrawlerServiceOptions> options,
        IBrowserFactory browserFactory)
    {
        _logger = logger;
        _pageParser = pageParser;
        _crawlingSchema = crawlingSchema;
        _browserFactory = browserFactory;
        _options = options.Value;
    }
    
    public Task<CrawlingResult> ParseLinkAsync(string link, CancellationToken cancellationToken = default)
    {
        return Policy.Handle<PageOpenException>()
            .WaitAndRetryAsync(10, i => TimeSpan.FromSeconds(i * 30))
            .ExecuteAsync(() => ParseLinkInternalAsync(link, cancellationToken));
    }

    protected abstract Task SetupBrowserPageAsync(IPage page);

    private async Task<CrawlingResult> ParseLinkInternalAsync(
        string link,
        CancellationToken cancellationToken = default)
    {
        if (_page is null)
        {
            var browser = await _browserFactory.GetInstanceAsync();

            _page = await browser.NewPageAsync();
            await SetupBrowserPageAsync(_page);
        }

        var random = new Random();
        IResponse? result;
        
        try
        {
            result = await _page.GoToAsync(link, WaitUntilNavigation.DOMContentLoaded);

            var timeToSleep = random.Next(
                _options.MinTimeoutBeforeSwitchToNextPage,
                _options.MaxTimeoutBeforeSwitchToNextPage);
                    
            _logger.LogInformation(
                "Wait for {Time} ms before the page crawling",
                timeToSleep);
            
            await Task.Delay(timeToSleep, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while opening the page {Link}", link);

            throw new PageOpenException("Parsing failed, page hasn't been opened", e);
        }

        if (result.Status >= HttpStatusCode.BadRequest)
        {
            throw new PageOpenException($"Parsing failed, page opened with status code: {result.Status}");
        }
        
        // Last page parsed. Cian specific, shouldn't be here
        if (result?.Url != link)
        {
            throw new SessionInterruptedException($"Redirect to {result?.Url} received. All pages have been parsed.");
        }

        return await _pageParser.ParseAsync(_page, _crawlingSchema);
    }
}