using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.GpuWorkerHost.Jobs;
using Laraue.Apps.RealEstate.Prediction.AppServices;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.DateTime.Services.Impl;
using Laraue.Core.Extensions.Hosting;
using Laraue.Core.Extensions.Hosting.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Configure system services
services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
services.AddScoped<IJobsDbContext>(sp => sp.GetRequiredService<AdvertisementsDbContext>());

services.AddOptions<PredictionOptions>();
services.Configure<PredictionOptions>(builder.Configuration.GetSection("PredictionOptions"));

// Configure database
services.AddDbContext<AdvertisementsDbContext>(s =>
    s.UseNpgsql(builder.Configuration.GetConnectionString("Postgre"))
        .UseSnakeCaseNamingConvention());

// Configure prediction services
services.AddScoped<IPredictor, OllamaRealEstatePredictor>();
services.AddHttpClient<IRemoteImagesPredictor, RemoteImagesPredictor>();
services.AddHttpClient<IOllamaPredictor, OllamaPredictor>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<PredictionOptions>>().Value;
    
    client.BaseAddress = new Uri(options.OllamaBaseAddress);
    client.Timeout = options.Timeout;
});

services.AddScoped<EstimateImagesRenovationJob.IRepository, EstimateImagesRenovationJob.Repository>();
services
    .AddBackgroundJob<EstimateImagesRenovationJob, EmptyJobData>(
        "EstimateImagesRenovationJob");

var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AdvertisementsDbContext>();
await dbContext.Database.MigrateAsync();

await app.RunAsync();