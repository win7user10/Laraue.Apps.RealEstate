using System.Reflection;
using System.Text.Json;
using Laraue.Apps.RealEstate.Db.Models;
using Laraue.Core.Extensions.Hosting.EfCore;
using Laraue.Telegram.NET.Interceptors.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Advertisement = Laraue.Apps.RealEstate.Db.Models.Advertisement;
using TransportStop = Laraue.Apps.RealEstate.Db.Models.TransportStop;

namespace Laraue.Apps.RealEstate.Db;

public sealed class AdvertisementsDbContext : DbContext, IJobsDbContext, IInterceptorsDbContext<Guid>
{
    public AdvertisementsDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Advertisement> Advertisements { get; init; }
    
    public DbSet<AdvertisementTransportStop> AdvertisementTransportStops { get; init; }
    
    public DbSet<Image> Images { get; init; }
    public DbSet<AdvertisementImage> AdvertisementImages { get; init; }
    
    public DbSet<TransportStop> TransportStops { get; init; }
    
    public DbSet<JobStateEntity> JobStates { get; set; }
    
    public DbSet<City> Cities { get; set; }
    public DbSet<Street> Streets { get; set; }
    public DbSet<House> Houses { get; set; }
    public DbSet<CrawlingSession> CrawlingSessions { get; init; }
    
    public DbSet<CrawlingSessionAdvertisement> CrawlingSessionAdvertisements { get; init; }
    public DbSet<Selection> Selections { get; init; }
    public DbSet<User> Users { get; init; }
    public DbSet<InterceptorStateModel<Guid>> InterceptorState { get; set; }
    
    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>()
            .HavePrecision(18, 4);
        
        configurationBuilder
            .Properties<MayBeRelativeDate>()
            .HaveConversion<MayBeRelativeDateConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_trgm");
        
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasIndex(x => x.Url).IsUnique();
            entity.HasIndex(x => x.LastAvailableAt);
        });
        
        modelBuilder.Entity<AdvertisementImage>()
            .HasKey(x => new { x.AdvertisementId, x.ImageId });

        modelBuilder.Entity<AdvertisementTransportStop>()
            .HasIndex(x => new { x.TransportStopId, x.AdvertisementId })
            .IsUnique();
        
        modelBuilder.Entity<JobStateEntity>()
            .HasKey(x => x.JobName);

        modelBuilder.Entity<Advertisement>(entity =>
        {
            entity.HasIndex(x => x.SourceId);
            entity.HasIndex(x => x.UpdatedAt);
            entity.HasIndex(x => x.Square);
            entity.HasIndex(x => x.RoomsCount);
            entity.HasIndex(x => x.FloorNumber);
            entity.HasIndex(x => x.PredictedAt);
            entity.HasIndex(x => x.ReadyAt);
            
            entity
                .HasIndex(x => x.ShortDescription)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");
        });
        
        modelBuilder.Entity<CrawlingSessionAdvertisement>()
            .HasKey(x => new { x.AdvertisementId, x.CrawlingSessionId });

        modelBuilder.Entity<Selection>()
            .HasIndex(x => x.SentAt);

        using var fileStream = File.OpenRead(Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Data/metro.json"));

        var metroStations = JsonSerializer.Deserialize<CianMetroStation[]>(
            fileStream, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        for (var i = 0; i < metroStations.Length; i++)
        {
            var metroStation = metroStations[i];
            modelBuilder.Entity<TransportStop>()
                .HasData(new TransportStop
                {
                    Id = i + 1,
                    Color = metroStation.Color,
                    Name = metroStation.Name,
                    Priority = metroStation.Priority,
                });
        }

        modelBuilder.Entity<City>(builder =>
        {
            builder.HasData(new City
            {
                Id = 1,
                Name = "Санкт-Петербург"
            });
            
            builder.HasData(new City
            {
                Id = 2,
                Name = "Волгоград"
            });
        });
        
        modelBuilder.Entity<Street>(builder =>
        {
            builder.HasIndex(x => new { x.CityId, x.Name }).IsUnique();
        });
        
        modelBuilder.Entity<House>(builder =>
        {
            builder.HasIndex(x => new { x.StreetId, x.Number }).IsUnique();
        });
    }

    private sealed record CianMetroStation
    {
        public int CianId { get; init; }
        public string Name { get; init; } = default!;
        public string Color { get; init; } = default!;
        public byte Priority { get; init; }
    }
}

public class CianCrawlerDbContextFactory : IDesignTimeDbContextFactory<AdvertisementsDbContext>
{
    public AdvertisementsDbContext CreateDbContext(string[] args)
    {
        return new AdvertisementsDbContext(new DbContextOptionsBuilder()
            .UseNpgsql("User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=cian_crawler;Command Timeout=0")
            .UseSnakeCaseNamingConvention()
            .Options);
    }
}