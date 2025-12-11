using Laraue.Apps.RealEstate.AppServices.Services;
using Laraue.Apps.RealEstate.AppServices.TransportStops;
using Laraue.Apps.RealEstate.Contracts;
using Laraue.Apps.RealEstate.Crawling.AppServices;
using Laraue.Apps.RealEstate.CrawlingHost;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.DataAccess.Storage;
using Laraue.Apps.RealEstate.Prediction.AppServices;
using Laraue.Core.DataAccess.Linq2DB.Extensions;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.DateTime.Services.Impl;
using Laraue.Core.Extensions.Hosting.EfCore;
using Laraue.Crawling.Dynamic.PuppeterSharp;
using Laraue.Crawling.Dynamic.PuppeterSharp.Abstractions;
using Laraue.Crawling.Dynamic.PuppeterSharp.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PuppeteerSharp;

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
services.AddScoped<IHousesStorage, HousesService>();
services.AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));
services.AddSingleton<IAdvertisementComputedFieldsCalculator, AdvertisementComputedFieldsCalculator>();
services.AddSingleton<IPageParser, PageParser>();
services.AddSingleton<ISessionInterrupter, SessionInterrupter>();

services.AddLinq2Db();

// Configure crawlers
services.AddCianCrawlers(builder.Configuration);
services.AddAvitoCrawlers(builder.Configuration);

services.AddScoped<IAdvertisementService, AdvertisementService>();
services.AddControllers();

// Build the app
var app = builder.Build();

app.Services.UseLinq2Db();
app.MapControllers();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AdvertisementsDbContext>();
await dbContext.Database.MigrateAsync();

app.Run();