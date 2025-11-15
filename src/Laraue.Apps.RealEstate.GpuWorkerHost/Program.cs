using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.GpuWorkerHost.Jobs;
using Laraue.Apps.RealEstate.Prediction.Abstractions;
using Laraue.Apps.RealEstate.Prediction.Impl;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.DateTime.Services.Impl;
using Laraue.Core.Extensions.Hosting;
using Laraue.Core.Extensions.Hosting.EfCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Configure system services
services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
services.AddScoped<IJobsDbContext>(sp => sp.GetRequiredService<AdvertisementsDbContext>());

// Configure database
services.AddDbContext<AdvertisementsDbContext>(s =>
    s.UseNpgsql(builder.Configuration.GetConnectionString("Postgre"))
        .UseSnakeCaseNamingConvention());

// Configure prediction services
services.AddScoped<IPredictor, OllamaRealEstatePredictor>();
services.AddHttpClient<IRemoteImagesPredictor, RemoteImagesPredictor>();
services.AddHttpClient<IOllamaPredictor, OllamaPredictor>(x =>
{
    x.BaseAddress = new Uri("http://localhost:11434/");
    x.Timeout = TimeSpan.FromSeconds(30);
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