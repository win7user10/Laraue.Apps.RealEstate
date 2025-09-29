using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.Avito;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Crawling.Abstractions;
using Laraue.Crawling.Abstractions.Schema.Binding;
using Laraue.Crawling.Common.Impl;
using Laraue.Crawling.Dynamic.PuppeterSharp;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace Laraue.Apps.RealEstate.Crawling.Impl.Avito;

public sealed class AvitoCrawlingSchema : CompiledDocumentSchema<IElementHandle, HtmlSelector, CrawlingResult>, IAvitoCrawlingSchema
{
    public AvitoCrawlingSchema(IDateTimeProvider dateTimeProvider, ILogger<AvitoCrawlingSchema> logger)
        : base(GetSchema(dateTimeProvider, logger))
    {
    }
    
    private static BindObjectExpression<IElementHandle, HtmlSelector> GetSchema(IDateTimeProvider dateTimeProvider, ILogger logger)
    {
        return new PuppeterSharpSchemaBuilder<CrawlingResult>()
            .HasArrayProperty(x => x.Advertisements, "div.items-items-kAJAg > div[data-marker=item]", pageBuilder =>
            {
                pageBuilder.HasProperty(
                    x => x.Id,
                    "data-item-id");
                pageBuilder.HasProperty(
                    x => x.Link,
                    builder => builder
                        .UseSelector("a")
                        .GetInnerTextFromAttribute("href"));
                pageBuilder.HasProperty(
                    x => x.ShortDescription,
                    ".iva-item-descriptionStep-C0ty1");
                pageBuilder.BindManually((element, binder) => binder
                    .BindPropertyAsync(x => x.Id, element.GetAttributeValueAsync("data-item-id")));
                pageBuilder.HasProperty(
                    x => x.TotalPrice,
                    builder => builder
                        .UseSelector("meta[itemprop=price]")
                        .GetInnerTextFromAttribute("content"));
                pageBuilder.HasProperty(
                    x => x.SquareMeterPrice,
                    builder => builder.UseSelector("a"));
                pageBuilder.HasProperty(
                    x => x.UpdatedAt,
                    builder => builder
                        .UseSelector(".iva-item-dateInfoStep-_acjp")
                        .Map(s => AvitoDateParser.Parse(s, dateTimeProvider)));
                pageBuilder.BindManually(async (element, binder) =>
                {
                    var slider = await element.QuerySelectorAsync("a.iva-item-sliderLink-uLz1v");
                    await slider.HoverAsync();
                    try
                    {
                        await element.WaitForSelectorAsync(
                            "ul > li:nth-child(2)",
                            new WaitForSelectorOptions
                            {
                                Timeout = 500,
                            });
                    }
                    catch (WaitTaskTimeoutException)
                    {
                    }

                    var images = await slider.QuerySelectorAllAsync("img");
                    var imagesLinks = await Task.WhenAll(images.Select(x => x.GetAttributeValueAsync("src")));
                    binder.BindProperty(x => x.ImageLinks, imagesLinks);
                });
                
                pageBuilder.BindManually(async (element, modelBinder) =>
                {
                    var titleElement = await element.QuerySelectorAsync("h3");
                    
                    var title = await titleElement.GetInnerTextAsync();

                    var extractResult = AvitoTitleExtractor.Extract(title);
                    if (extractResult.RoomsNumber is not null)
                    {
                        modelBinder.BindProperty(x => x.RoomsCount, extractResult.RoomsNumber);
                    }
                    
                    if (extractResult.Floor is not null)
                    {
                        modelBinder.BindProperty(x => x.FloorNumber, extractResult.Floor);
                    }
                    
                    if (extractResult.TotalFloors is not null)
                    {
                        modelBinder.BindProperty(x => x.TotalFloorsNumber, extractResult.TotalFloors);
                    }
                    
                    logger.LogInformation(
                        "Adv {Id}, data bind result: {Result}",
                        modelBinder.GetProperty(x => x.Id),
                        extractResult);
                });

                pageBuilder.BindManually(async (element, modelBinder) =>
                {
                    var geoInfo = await element.QuerySelectorAsync(".geo-root-zPwRk p:nth-child(2)");
                    
                    if (geoInfo is null)
                    {
                        return;
                    }

                    var nameElement = await geoInfo.QuerySelectorAsync("span:nth-child(2)");
                    var name = await nameElement.GetInnerTextAsync();

                    if (name is null)
                    {
                        return;
                    }

                    var transportStop = new TransportStop { Name = name };
                    var transportStops = new[] { transportStop };
                    
                    var timeElement = await geoInfo.QuerySelectorAsync("span:nth-child(3)");
                    if (timeElement is null)
                    {
                        modelBinder.BindProperty(x => x.TransportStops, transportStops);
                        return;
                    }

                    var svgElement = await timeElement.QuerySelectorAsync("svg");
                    if (svgElement is null)
                    {
                        modelBinder.BindProperty(x => x.TransportStops, transportStops);
                        return;
                    }

                    var svgName = await svgElement.GetAttributeValueAsync("name");
                    var distanceType = svgName switch
                    {
                        "walking-route" => DistanceType.Foot,
                        _ => DistanceType.Car
                    };
                    
                    var timeString = await timeElement.GetInnerTextAsync();
                    var time = AvitoTimeParser.Parse(timeString);
                    if (time is not null)
                    {
                        transportStops[0] = transportStop with
                        {
                            DistanceType = distanceType,
                            Minutes = time.Value
                        };
                    }
                    
                    modelBinder.BindProperty(x => x.TransportStops, transportStops);
                });

                pageBuilder.ExecuteAsync(_ => Task.Delay(500));
            })
            .Build()
            .BindingExpression;
    }
}