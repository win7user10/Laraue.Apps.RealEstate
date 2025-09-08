using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.TransportStops;
using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.Db.Storage;
using Laraue.Core.DataAccess.Linq2DB.Extensions;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Laraue.Apps.RealEstate.IntegrationTests;

[Collection("Database Tests")]
public abstract class TestWithDatabase
{
    protected readonly AdvertisementsDbContext DbContext;
    
    protected TestWithDatabase()
    {
        DbContext = new AdvertisementsDbContext(new DbContextOptionsBuilder()
            .UseNpgsql("User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=test_real_estate;Command Timeout=0")
            .UseSnakeCaseNamingConvention()
            .Options);
        
        LinqToDBForEFTools.Initialize();
        DbContext.Database.Migrate();
        ClearDb();
        SeedDb();
    }

    private void ClearDb()
    {
        DbContext.Advertisements.Delete();
        DbContext.CrawlingSessions.Delete();
    }
    
    private void SeedDb()
    {
        DbContext.SaveChanges();
    }

    protected IServiceCollection ServiceCollection
    {
        get
        {
            var sc = new ServiceCollection()
                .AddSingleton<AdvertisementsDbContext>(_ => DbContext)
                .AddSingleton<IMetroStationsStorage, MetroStationsStorage>()
                .AddLinq2Db();

            return sc;
        }
    }
}