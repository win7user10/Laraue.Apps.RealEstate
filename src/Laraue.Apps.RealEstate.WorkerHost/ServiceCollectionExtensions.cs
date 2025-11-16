using Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.Avito;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.Cian;
using Laraue.Apps.RealEstate.Crawling.Impl.Avito;
using Laraue.Apps.RealEstate.Crawling.Impl.Cian;
using Laraue.Crawling.Crawler.EfCore;

namespace Laraue.Apps.RealEstate.WorkerHost;

public static class ServiceCollectionExtensions
{
    public static void AddCianCrawler(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CianCrawlerServiceOptions>();
        services.Configure<CianCrawlerServiceOptions>(configuration.GetRequiredSection("Crawlers:Cian"));

        services
            .AddSingleton<ICianCrawlingSchema, CianCrawlingSchema>()
            .AddSingleton<ICianCrawlingSchemaParser, CianCrawlingSchemaParser>()
            .AddScoped<ICianAdvertisementProcessor, CianAdvertisementProcessor>()
            .AddCrawlingService<CianCrawlerJob, CrawlingResult, string, State>("CianCrawler");
    }
    
    public static void AddAvitoCrawler(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AvitoCrawlerServiceOptions>();
        services.Configure<AvitoCrawlerServiceOptions>(configuration.GetRequiredSection("Crawlers:Avito"));

        services
            .AddSingleton<IAvitoCrawlingSchema, AvitoCrawlingSchema>()
            .AddSingleton<IAvitoCrawlingSchemaParser, AvitoCrawlingSchemaParser>()
            .AddScoped<IAvitoAdvertisementProcessor, AvitoAdvertisementProcessor>()
            .AddCrawlingService<AvitoCrawlerJob, CrawlingResult, string, State>("AvitoCrawler");
    }
}