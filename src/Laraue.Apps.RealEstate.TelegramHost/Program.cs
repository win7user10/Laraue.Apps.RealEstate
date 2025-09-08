using System.Reflection;
using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.Db.Models;
using Laraue.Apps.RealEstate.Db.Storage;
using Laraue.Apps.RealEstate.Telegram;
using Laraue.Apps.RealEstate.TelegramHost;
using Laraue.Apps.RealEstate.TelegramHost.Services;
using Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection;
using Laraue.Core.DataAccess.Linq2DB.Extensions;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.DateTime.Services.Impl;
using Laraue.Telegram.NET.Authentication.Extensions;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Core;
using Laraue.Telegram.NET.Core.Extensions;
using Laraue.Telegram.NET.Core.Middleware;
using Laraue.Telegram.NET.Interceptors.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<TelegramNetOptions>();
builder.Services.Configure<TelegramNetOptions>(builder.Configuration.GetSection("Telegram"));

builder.Services.AddTelegramCore();

builder.Services.AddDbContext<AdvertisementsDbContext>(s =>
    s.UseNpgsql(builder.Configuration.GetConnectionString("Postgre"))
        .UseSnakeCaseNamingConvention());
builder.Services.AddLinq2Db();

builder.Services.AddScoped<ITelegramMessageSender, TelegramMessageSender>();
builder.Services.AddScoped<UpdateInterceptorsFactory>();
builder.Services.AddScoped<IStorage, Storage>();
builder.Services.AddScoped<IAdvertisementStorage, AdvertisementStorage>();

builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddScoped<IAdvertisementsTelegramSender, AdvertisementsTelegramSender>();

builder.Services.AddTelegramRequestEfCoreInterceptors<Guid, AdvertisementsDbContext>(new []{Assembly.GetExecutingAssembly()});

builder.Services.AddTelegramAuthentication<User, Guid, TelegramUserQueryService, Request>();

builder.Services.AddOptions<RoleUsers>();
builder.Services.Configure<RoleUsers>(builder.Configuration.GetSection("Telegram:Groups"));
builder.Services.UseUserRolesProvider<StaticUserRoleProvider>();

builder.Services.AddTelegramMiddleware<AutoCallbackResponseMiddleware>();

builder.Services.AddHttpClient<IHealthChecker, HealthChecker>();
builder.Services.AddOptions<HealthOptions>();
builder.Services.Configure<HealthOptions>(builder.Configuration.GetSection("Health"));

var app = builder.Build();

app.Services.UseLinq2Db();
app.MapGet("/", () => "Hello World!");

using var scope = app.Services.CreateScope();

app.MapTelegramRequests();

var dbContext = scope.ServiceProvider.GetRequiredService<AdvertisementsDbContext>();
await dbContext.Database.MigrateAsync();

var tgClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
await tgClient.SetMyCommands(StaticBotMenu.Menu);

app.Run();