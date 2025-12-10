using Laraue.Apps.RealEstate.Crawling.AppServices.Avito;
using Laraue.Apps.RealEstate.Crawling.AppServices.Cian;
using Laraue.Apps.RealEstate.Crawling.Contracts;
using Laraue.Crawling.Crawler.EfCore;

namespace Laraue.Apps.RealEstate.CrawlingHost;

public static class ServiceCollectionExtensions
{
    private const string AvitoSection = "Crawlers:Avito";
    private const string CianSection = "Crawlers:Cian";
    
    public static void AddCianCrawlers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CianCrawlerServiceOptions>();
        services.Configure<CianCrawlerServiceOptions>(configuration.GetRequiredSection(CianSection));

        services
            .AddSingleton<ICianCrawlingSchema, CianCrawlingSchema>()
            .AddSingleton<ICianCrawlingSchemaParser, CianCrawlingSchemaParser>()
            .AddScoped<ICianAdvertisementProcessor, CianAdvertisementProcessor>();
        
        var options = configuration
            .GetRequiredSection(CianSection)
            .Get<AvitoCrawlerServiceOptions>()
                ?? throw new InvalidOperationException();
        
        foreach (var city in options.Cities)
        {
            services.AddCrawlingService<CianCrawlerJob, CrawlingResult, string, State>(
                $"CianCrawler_City_{city.CityId}",
                city.CityId,
                city.CrawlingUrl);
        }
    }
    
    public static void AddAvitoCrawlers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AvitoCrawlerServiceOptions>();
        services.Configure<AvitoCrawlerServiceOptions>(configuration.GetRequiredSection(AvitoSection));

        services
            .AddSingleton<IAvitoCrawlingSchema, AvitoCrawlingSchema>()
            .AddSingleton<IAvitoCrawlingSchemaParser, AvitoCrawlingSchemaParser>()
            .AddScoped<IAvitoAdvertisementProcessor, AvitoAdvertisementProcessor>();
        
        var options = configuration
            .GetRequiredSection(AvitoSection)
            .Get<AvitoCrawlerServiceOptions>()
                ?? throw new InvalidOperationException();
        
        foreach (var city in options.Cities)
        {
            services.AddCrawlingService<AvitoCrawlerJob, CrawlingResult, string, State>(
                $"AvitoCrawler_City_{city.CityId}",
                city.CityId,
                city.CrawlingUrl);
        }
    }
}