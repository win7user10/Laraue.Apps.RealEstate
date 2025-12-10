using Laraue.Apps.RealEstate.Contracts;
using Laraue.Apps.RealEstate.Crawling.Contracts;
using Laraue.Crawling.Abstractions;
using Laraue.Crawling.Abstractions.Schema.Binding;
using Laraue.Crawling.Common.Extensions;
using Laraue.Crawling.Common.Impl;
using Laraue.Crawling.Dynamic.PuppeterSharp;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace Laraue.Apps.RealEstate.Crawling.AppServices.Cian;

public sealed class CianCrawlingSchema : CompiledDocumentSchema<IElementHandle, HtmlSelector, CrawlingResult>, ICianCrawlingSchema
{
    public CianCrawlingSchema(ILogger<CianCrawlingSchema> logger)
        : base(GetSchema(logger))
    {
    }
    
    private static BindObjectExpression<IElementHandle, HtmlSelector> GetSchema(ILogger logger)
    {
        return new PuppeterSharpSchemaBuilder<CrawlingResult>()
            .HasArrayProperty(x => x.Advertisements, "article", pageBuilder =>
            {
                pageBuilder.HasProperty(
                    x => x.ShortDescription,
                    "div[data-name=Description]");
                pageBuilder.BindManually(async (e, b) =>
                {
                    var linkElement = await e.QuerySelectorAsync("div[data-name=LinkArea] a");
                    var href = await linkElement.GetAttributeValueAsync("href");

                    if (href is null)
                    {
                        return;
                    }
                    
                    b.BindProperty(x => x.Id, href.GetIntOrDefault().ToString());
                    b.BindProperty(x => x.Link, new Uri(href).LocalPath);
                });
                pageBuilder.HasProperty(
                    x => x.TotalPrice,
                    builder => builder
                        .UseSelector("span[data-mark=MainPrice]")
                        .Map(s => long.Parse(s.GetOnlyDigits())));
                pageBuilder.HasProperty(
                    x => x.UpdatedAt,
                    builder => builder
                        .UseSelector("div[data-name=TimeLabel] div:nth-child(2)")
                        .Map(TryParseDate));
                pageBuilder.HasArrayProperty(
                    x => x.ImageLinks,
                    "div[data-name=Gallery] img",
                    el => el!.GetAttributeValueAsync("src"));
                
                pageBuilder.BindManually(async (element, modelBinder) =>
                {
                    var titleElement = await element.QuerySelectorAsync("span[data-mark=OfferTitle]");
                    var subTitleElement = await element.QuerySelectorAsync("span[data-mark=OfferSubtitle]");
                    
                    var title = await titleElement.GetInnerTextAsync();
                    var subTitle = await subTitleElement.GetInnerTextAsync();

                    var extractResult = CianTitleExtractor.ExtractTitle(title, subTitle);
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
                    var addressDomElements = await element.QuerySelectorAllAsync("a[data-name=GeoLabel]");
                    var addressElements = new List<string>();
                    
                    foreach (var addressDomElement in addressDomElements)
                    {
                        addressElements.Add(await addressDomElement.GetInnerTextAsync() ?? string.Empty);
                    }

                    if (addressElements.Count < 2)
                    {
                        return;
                    }

                    var lastElements = addressElements.TakeLast(2).ToArray();
                    modelBinder.BindProperty(x => x.FlatAddress, new FlatAddress
                    {
                        Street = lastElements.First(),
                        HouseNumber = lastElements.Last()
                    });
                });

                pageBuilder.BindManually(async (element, modelBinder) =>
                {
                    var name = await element.QuerySelectorAsync("div[data-name=SpecialGeo] a")
                        .AwaitAndModify(x => x.GetInnerTextAsync());
                    
                    if (name is null)
                    {
                        return;
                    }

                    var transportStop = new TransportStop
                    {
                        Name = name
                    };
                    
                    var subElement = await element.QuerySelectorAsync("div[data-name=SpecialGeo] > div");
                    if (subElement is null)
                    {
                        modelBinder.BindProperty(
                            x => x.TransportStops,
                            new [] { transportStop });
                        
                        return;
                    }
                    
                    // 7 минут пешком or 5 минут на транспорте
                    var title = await subElement.GetInnerTextAsync();
                    var titleParts = title?.Split(' ') ?? Array.Empty<string>();
                    if (titleParts.Length < 2)
                    {
                        modelBinder.BindProperty(
                            x => x.TransportStops,
                            new [] { transportStop });
                        
                        return;
                    }
                    
                    var minutesToMetro = titleParts[0].GetIntOrDefault();
                    var distanceType = titleParts.Last() == "пешком" ? DistanceType.Foot : DistanceType.Car;
                    
                    modelBinder.BindProperty(
                        x => x.TransportStops,
                        new [] 
                        {
                            transportStop with
                            {
                                Minutes = minutesToMetro,
                                DistanceType = distanceType,
                            }
                        });
                });
            })
            .Build()
            .BindingExpression;
    }
    
    private static DateTime? TryParseDate(string? strDate)
    {
        // сегодня, 13:40, 24 сен, 14:50, вчера, 23:10

        var dateParts = strDate?.Split(", ") ?? [];

        if (dateParts.Length != 2)
        {
            return null;
        }

        var day = dateParts[0];
        var date = day switch
        {
            "сегодня" => DateTime.Today,
            "вчера" => DateTime.Today.AddDays(-1),
            _ => ParseDate(day),
        };

        var time = TimeSpan.Parse(dateParts[1]);
        date = date.Add(time);

        return date.ToUniversalTime();
    }
    
    private static int GetMonth(string str)
    {
        return str switch
        {
            "янв" => 1,
            "фев" => 2,
            "мар" => 3,
            "апр" => 4,
            "май" => 5,
            "июн" => 6,
            "июл" => 7,
            "авг" => 8,
            "сен" => 9,
            "окт" => 10,
            "ноя" => 11,
            "дек" => 12,
            _ => throw new Exception(),
        };
    }

    private static DateTime ParseDate(string day)
    {
        var parts = day.Split(" ");
        var d = int.Parse(parts[0]);
        var m = GetMonth(parts[1]);
        var dt = new DateTime(DateTime.UtcNow.Year, m, d);
        return dt > DateTime.UtcNow ? dt.AddYears(-1) : dt;
    }
}