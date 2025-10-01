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
    private static readonly Dictionary<string, string> MetroMap = new()
    {
        ["Площадь А. Невского I"] = "Площадь Александра Невского",
        ["Площадь А. Невского II"] = "Площадь Александра Невского",
        ["Технологический ин-т I"] = "Технологический институт",
        ["Технологический ин-т II"] = "Технологический институт",
        ["Чёрная речка"] = "Черная речка",
        ["Звёздная"] = "Звездная",
    };
    public AvitoCrawlingSchema(IDateTimeProvider dateTimeProvider, ILogger<AvitoCrawlingSchema> logger)
        : base(GetSchema(dateTimeProvider, logger))
    {
    }
    
    private static BindObjectExpression<IElementHandle, HtmlSelector> GetSchema(IDateTimeProvider dateTimeProvider, ILogger logger)
    {
        return new PuppeterSharpSchemaBuilder<CrawlingResult>()
            .HasArrayProperty(x => x.Advertisements, "#bx_serp-item-list > div[data-marker=item]", pageBuilder =>
            {
                pageBuilder.HasProperty(
                    x => x.Id,
                    builder => builder
                        .GetInnerTextFromAttribute("data-item-id"));
                pageBuilder.HasProperty(
                    x => x.Link,
                    builder => builder
                        .UseSelector("a")
                        .GetInnerTextFromAttribute("href"));
                pageBuilder.HasProperty(
                    x => x.ShortDescription,
                    builder => builder
                        .UseSelector("meta[itemprop=description]")
                        .GetInnerTextFromAttribute("content"));
                pageBuilder.HasProperty(
                    x => x.TotalPrice,
                    builder => builder
                        .UseSelector("meta[itemprop=price]")
                        .GetInnerTextFromAttribute("content"));
                pageBuilder.HasProperty(
                    x => x.UpdatedAt,
                    builder => builder
                        .UseSelector(".iva-item-dateInfoStep-AoWrh")
                        .Map(s => AvitoDateParser.Parse(s, dateTimeProvider)));

                pageBuilder.HasArrayProperty(
                    x => x.ImageLinks,
                    "li.photo-slider-list-item-r2YDC",
                    element => element.GetAttributeValueAsync("data-marker")
                        .AwaitAndModify(v => v?.Replace("slider-image/image-", string.Empty)));
                
                pageBuilder.BindManually(async (element, modelBinder) =>
                {
                    var titleElement = await element.QuerySelectorAsync("h2");
                    
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
                    
                    if (extractResult.Square is not null)
                    {
                        modelBinder.BindProperty(x => x.Square, extractResult.Square.Value);
                    }
                    
                    logger.LogInformation(
                        "Adv {Id}, data bind result: {Result}",
                        modelBinder.GetProperty(x => x.Id),
                        extractResult);
                });

                pageBuilder.BindManually(async (element, modelBinder) =>
                {
                    var geoInfo = await element.QuerySelectorAsync(".geo-root-BBVai p:nth-child(2)");
                    
                    if (geoInfo is null)
                    {
                        return;
                    }

                    var nameElement = await geoInfo.QuerySelectorAsync("a");
                    var name = await nameElement.GetInnerTextAsync();

                    if (name is null)
                    {
                        return;
                    }

                    if (MetroMap.TryGetValue(name, out var mappedName))
                    {
                        name = mappedName;
                    }
                    
                    var transportStop = new TransportStop { Name = name };
                    var transportStops = new[] { transportStop };
                    
                    var timeElement = await geoInfo.QuerySelectorAsync("span:nth-child(2)");
                    if (timeElement is null)
                    {
                        modelBinder.BindProperty(x => x.TransportStops, transportStops);
                        return;
                    }
                    
                    var timeString = await timeElement.GetInnerTextAsync();
                    var time = AvitoTimeParser.Parse(timeString);
                    if (time is not null)
                    {
                        transportStops[0] = transportStop with
                        {
                            DistanceType = DistanceType.Foot,
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