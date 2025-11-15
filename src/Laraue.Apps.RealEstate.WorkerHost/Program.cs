using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.TransportStops;
using Laraue.Apps.RealEstate.Crawling.Impl;
using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.Db.Storage;
using Laraue.Apps.RealEstate.Telegram;
using Laraue.Apps.RealEstate.WorkerHost;
using Laraue.Apps.RealEstate.WorkerHost.Jobs;
using Laraue.Core.DataAccess.Linq2DB.Extensions;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.DateTime.Services.Impl;
using Laraue.Core.Extensions.Hosting;
using Laraue.Core.Extensions.Hosting.EfCore;
using Laraue.Crawling.Dynamic.PuppeterSharp;
using Laraue.Crawling.Dynamic.PuppeterSharp.Abstractions;
using Laraue.Crawling.Dynamic.PuppeterSharp.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

var launchOptions = builder
    .Configuration
    .GetRequiredSection("LaunchOptions")
    .Get<LaunchOptions>()
        ?? throw new InvalidOperationException("Launch browser options should be set");

services.AddCrawlingServices(launchOptions);

// Configure system services
services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

// Configure database
services.AddDbContext<AdvertisementsDbContext>(s =>
    s.UseNpgsql(builder.Configuration.GetConnectionString("Postgre"))
        .UseSnakeCaseNamingConvention());

services.AddScoped<IJobsDbContext>(sp => sp.GetRequiredService<AdvertisementsDbContext>());
services.AddScoped<IMetroStationsStorage, MetroStationsStorage>();
services.AddScoped<UpdateAdvertisementsPredictionJob.IRepository, UpdateAdvertisementsPredictionJob.Repository>();
services.AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));
services.AddSingleton<IAdvertisementComputedFieldsCalculator, AdvertisementComputedFieldsCalculator>();
services.AddSingleton<IPageParser, PageParser>();

services.AddLinq2Db();

// Configure crawlers
services.AddCianCrawler(builder.Configuration);
services.AddAvitoCrawler(builder.Configuration);

// Configure advertisements sending
services.AddOptions<AdvertisementsSenderOptions>();
services.Configure<AdvertisementsSenderOptions>(builder.Configuration.GetRequiredSection("Telegram"));

services.AddSingleton<ITelegramBotClient>(
    sp => new TelegramBotClient(
        sp.GetRequiredService<IOptions<AdvertisementsSenderOptions>>().Value.Token));

services.AddScoped<IAdvertisementsTelegramSender, AdvertisementsTelegramSender>();
services.AddScoped<IAdvertisementStorage, AdvertisementStorage>();
        
services.AddScoped<IPublicAdvertisementsPicker, PublicAdvertisementsPicker>();

services
    .AddBackgroundJob<SendPublicAdvertisementsJob, SendPublicAdvertisementsJobContext>(
        "SendPublicAdvertisementsJob");
services
    .AddBackgroundJob<SendSelectionsAdvertisementsJob, EmptyJobData>(
        "SendSelectionsAdvertisementsJob");
services
    .AddBackgroundJob<UpdateAdvertisementsPredictionJob, EmptyJobData>(
        "UpdateAdvertisementsPredictionJob");

services.AddControllers();

// Build the app
var app = builder.Build();

app.Services.UseLinq2Db();
app.MapControllers();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AdvertisementsDbContext>();
await dbContext.Database.MigrateAsync();

app.Run();