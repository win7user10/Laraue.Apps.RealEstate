using Laraue.Apps.RealEstate.AppServices.Telegram;
using Laraue.Apps.RealEstate.Contracts;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.DataAccess.Storage;
using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.Prediction.Abstractions;
using Laraue.Apps.RealEstate.Prediction.Impl;
using Laraue.Apps.RealEstate.WorkerHost.Jobs;
using Laraue.Core.DataAccess.Linq2DB.Extensions;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.DateTime.Services.Impl;
using Laraue.Core.Extensions.Hosting;
using Laraue.Core.Extensions.Hosting.EfCore;
using Laraue.Crawling.Dynamic.PuppeterSharp;
using Laraue.Crawling.Dynamic.PuppeterSharp.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Configure system services
services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

// Configure database
services.AddDbContext<AdvertisementsDbContext>(s =>
    s.UseNpgsql(builder.Configuration.GetConnectionString("Postgre"))
        .UseSnakeCaseNamingConvention());

services.AddScoped<IJobsDbContext>(sp => sp.GetRequiredService<AdvertisementsDbContext>());
services.AddScoped<IHousesStorage, HousesStorage>();
services.AddScoped<UpdateAdvertisementsPredictionJob.IRepository, UpdateAdvertisementsPredictionJob.Repository>();
services.AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));
services.AddSingleton<IAdvertisementComputedFieldsCalculator, AdvertisementComputedFieldsCalculator>();

services.AddLinq2Db();

// Configure advertisements sending
services.AddOptions<AdvertisementsSenderOptions>();
services.Configure<AdvertisementsSenderOptions>(builder.Configuration.GetRequiredSection("Telegram"));

services.AddSingleton<ITelegramBotClient>(
    sp => new TelegramBotClient(
        sp.GetRequiredService<IOptions<AdvertisementsSenderOptions>>().Value.Token));

services.AddScoped<IAdvertisementsTelegramSender, AdvertisementsTelegramSender>();
services.AddScoped<IAdvertisementStorage, AdvertisementStorage>();
        
services.AddScoped<IPublicAdvertisementsPicker, PublicAdvertisementsPicker>();

services.AddBackgroundJob<SendPublicAdvertisementsJob, SendPublicAdvertisementsJobContext>("SendPublicAdvertisementsJob");
services
    .AddBackgroundJob<SendSelectionsAdvertisementsJob, EmptyJobData>(
        "SendSelectionsAdvertisementsJob");
services
    .AddBackgroundJob<UpdateAdvertisementsPredictionJob, EmptyJobData>(
        "UpdateAdvertisementsPredictionJob");
services
    .AddBackgroundJob<CleanUnavailableLinksJob, EmptyJobData>(
        "CleanUnavailableLinksJob");
services.AddHttpClient<CleanUnavailableLinksJob>();

services
    .AddBackgroundJob<ArchiveAdvertisementsWithoutImagesJob, EmptyJobData>(
        "ArchiveAdvertisementsWithoutImagesJob");

services.AddControllers();

// Build the app
var app = builder.Build();

app.Services.UseLinq2Db();
app.MapControllers();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AdvertisementsDbContext>();
await dbContext.Database.MigrateAsync();

app.Run();