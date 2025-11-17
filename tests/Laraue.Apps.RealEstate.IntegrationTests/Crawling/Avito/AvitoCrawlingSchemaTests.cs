using System.Net;
using Laraue.Apps.RealEstate.Crawling.Impl.Avito;
using Laraue.Core.DateTime.Services.Impl;
using Laraue.Core.Testing.Logging;
using Laraue.Crawling.Dynamic.PuppeterSharp;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using Xunit;
using Xunit.Abstractions;

namespace Laraue.Apps.RealEstate.IntegrationTests.Crawling.Avito;

public sealed class AvitoCrawlingSchemaTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly PuppeterSharpParser _parser;
    private IBrowser? _browser;
    private IPage? _page;

    public AvitoCrawlingSchemaTests(ITestOutputHelper outputHelper)
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
        
    }
    
    [Fact]
    public async Task SmokeAsync()
    {
        var response = await _page!.GoToAsync("https://www.avito.ru/sankt-peterburg/kvartiry/prodam/vtorichka-ASgBAgICAkSSA8YQ5geMUg?cd=1&context=H4sIAAAAAAAA_wEjANz_YToxOntzOjg6ImZyb21QYWdlIjtzOjc6ImNhdGFsb2ciO312FITcIwAAAA&f=ASgBAQICAkSSA8YQ5geMUgFAkL4NJJSuNZauNQ&localPriority=0&s=104", WaitUntilNavigation.DOMContentLoaded);

        if (response.Status > HttpStatusCode.Redirect)
        {
            throw new Exception(response.Status.ToString());
        }

        var schema = new AvitoCrawlingSchema(new DateTimeProvider(), new TestOutputHelperLogger<AvitoCrawlingSchema>(_outputHelper));

        var result = await _parser.RunAsync(schema, await _page.QuerySelectorAsync("body"));
        
        Assert.True(result!.Advertisements.Length > 5);
        Assert.All(result.Advertisements, x => Assert.NotNull(x.UpdatedAt));
    }

    public async Task DisposeAsync()
    {
        await _browser!.DisposeAsync();
    }
}