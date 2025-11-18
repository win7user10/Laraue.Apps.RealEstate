using Laraue.Apps.RealEstate.Crawling.Impl.Cian;
using Laraue.Crawling.Dynamic.PuppeterSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PuppeteerSharp;

namespace Laraue.Apps.RealEstate.UnitTests.Cian;

public class CianCrawlingSchemaTests : IAsyncLifetime
{
    private readonly PuppeterSharpParser _parser = new(new LoggerFactory());
    private IBrowser? _browser;
    private IPage? _page;

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
    }
    
    [Fact]
    public async Task Schema_ShouldReturnFilledFields_Always()
    {
        var content = await File.ReadAllTextAsync("html/cian.html");
        await _page!.SetContentAsync(content);

        var schema = new CianCrawlingSchema(new NullLogger<CianCrawlingSchema>());
        var result = await _parser.RunAsync(schema, await _page.QuerySelectorAsync("body"));

        var firstItem = result?.Advertisements.FirstOrDefault();
        Assert.NotNull(firstItem);
        
        Assert.NotNull(firstItem.FlatAddress);
        Assert.Equal("Большая Озерная улица", firstItem.FlatAddress.Street);
        Assert.Equal("59", firstItem.FlatAddress.HouseNumber);
    }

    public async Task DisposeAsync()
    {
        await _browser!.DisposeAsync();
    }
}