using Laraue.Apps.RealEstate.Crawling.Impl.Avito;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Crawling.Dynamic.PuppeterSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PuppeteerSharp;

namespace Laraue.Apps.RealEstate.UnitTests.Avito;

public class AvitoCrawlingSchemaTests : IAsyncLifetime
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
        var content = await File.ReadAllTextAsync("html/avito.html");
        await _page!.SetContentAsync(content);

        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider
            .Setup(x => x.UtcNow)
            .Returns(new DateTime(2020, 01, 01));
        
        var schema = new AvitoCrawlingSchema(dateTimeProvider.Object, new NullLogger<AvitoCrawlingSchema>());
        var result = await _parser.RunAsync(schema, await _page.QuerySelectorAsync("body"));

        var firstItem = result?.Advertisements.FirstOrDefault();
        Assert.NotNull(firstItem);
        
        Assert.NotNull(firstItem.FlatAddress);
        Assert.Equal("наб. Обводного канала", firstItem.FlatAddress.Street);
        Assert.Equal("169", firstItem.FlatAddress.HouseNumber);
    }

    public async Task DisposeAsync()
    {
        await _browser!.DisposeAsync();
    }
}