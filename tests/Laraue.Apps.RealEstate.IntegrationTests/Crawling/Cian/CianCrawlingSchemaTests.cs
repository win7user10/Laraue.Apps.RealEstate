using Laraue.Apps.RealEstate.Crawling.AppServices.Cian;
using Laraue.Core.Testing.Logging;
using Laraue.Crawling.Dynamic.PuppeterSharp;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Laraue.Apps.RealEstate.IntegrationTests.Crawling.Cian;

[IntegrationTest]
public sealed class CianCrawlingSchemaTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly PuppeterSharpParser _parser;
    private IBrowser? _browser;
    private IPage? _page;

    public CianCrawlingSchemaTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _parser = new PuppeterSharpParser(new LoggerFactory());
    }
    
    public async Task InitializeAsync()
    {
        await new BrowserFetcher().DownloadAsync();
        _browser = await Puppeteer.LaunchAsync(
            new LaunchOptions
            {
                DefaultViewport = new ViewPortOptions { Width = 1920, Height = 1024 },
                Headless = true
            });
        _page = await _browser.NewPageAsync();
        await _page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 Safari/537.36");
    }
    
    [Fact]
    public async Task SmokeAsync()
    {
        await _page!.GoToAsync("https://spb.cian.ru/cat.php?deal_type=sale&engine_version=2&offer_type=flat&region=2&sort=creation_date_desc");

        var schema = new CianCrawlingSchema(new TestOutputHelperLogger<CianCrawlingSchema>(_outputHelper));

        var result = await _parser.RunAsync(schema, await _page.QuerySelectorAsync("body"));
        
        Assert.True(result!.Advertisements.Length > 5);
        Assert.All(result.Advertisements, x =>
        {
            Assert.NotNull(x.UpdatedAt);
        });
    }

    public async Task DisposeAsync()
    {
        await _browser!.DisposeAsync();
    }
}