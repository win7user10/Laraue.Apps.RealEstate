using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Crawling.Impl.Cian;
using Laraue.Apps.RealEstate.Db.Models;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.DateTime.Services.Impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Advertisement = Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts.Advertisement;

namespace Laraue.Apps.RealEstate.IntegrationTests.Crawling.Cian;

public class CianAdvertisementProcessorTests : TestWithDatabase
{
    private readonly CianAdvertisementProcessor _processor;

    public CianAdvertisementProcessorTests()
    {
        var sp = ServiceCollection.AddSingleton<CianAdvertisementProcessor>()
            .AddSingleton<IDateTimeProvider, DateTimeProvider>()
            .AddSingleton(new Mock<ILogger<CianAdvertisementProcessor>>().Object)
            .BuildServiceProvider();

        _processor = sp.GetRequiredService<CianAdvertisementProcessor>();
    }

    [Fact]
    public async Task SaveNewItems_ShouldSaveThemIntoDatabaseAsync()
    {
        var advertisements = new Advertisement[]
        {
            new ()
            {
                Id = "1",
                FloorNumber = 2,
                ImageLinks = ["https://link1", "https://link2"],
                RoomsCount = 2,
                ShortDescription = "sh",
                TotalPrice = 10_000_000,
                TransportStops =
                [
                    new ()
                     {
                         Minutes = 10,
                         DistanceType = DistanceType.Foot,
                         Name = "Лесная"
                     }
                ],
                UpdatedAt = new DateTime(2020, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                TotalFloorsNumber = 9,
                Square = 30,
            }
        };
        
        await _processor.ProcessAsync(advertisements);

        var advertisement = Assert.Single(
            DbContext.Advertisements
                .Include(x => x.TransportStops));
        
        Assert.Equal("1", advertisement.SourceId);
        Assert.Equal(AdvertisementSource.Cian, advertisement.SourceType);
        
        var transportStop = Assert.Single(advertisement.TransportStops);
        Assert.Equal(DistanceType.Foot, transportStop.DistanceType);
        Assert.Equal(10, transportStop.DistanceInMinutes);
        Assert.Equal(6, transportStop.TransportStopId);
    }
    
    [Fact]
    public async Task SaveEarlySavedItems_ShouldUpdateThemInDatabaseAsync()
    {
        DbContext.Advertisements.Add(new Laraue.Apps.RealEstate.Db.Models.Advertisement
        {
            UpdatedAt = new DateTime(2022, 01, 01, 0, 0, 0, DateTimeKind.Utc),
            SourceType = AdvertisementSource.Cian,
            TotalPrice = 500_000,
            FloorNumber = 3,
            RoomsCount = 2,
            Square = 30,
            SourceId = "12",
            TotalFloorsNumber = 5,
            ShortDescription = "abc",
            LinkedImages = new List<AdvertisementImage>()
            {
                new() { Image = new () { Url = "https://link1" } },
                new() { Image = new () { Url = "https://link2" } },
            },
            TransportStops = new List<AdvertisementTransportStop>()
            {
                new () { DistanceType = DistanceType.Foot, DistanceInMinutes = 6, TransportStopId = 3 }
            },
        });

        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();
        
        var advertisements = new Advertisement[]
        {
            new ()
            {
                Id = "12",
                FloorNumber = 2,
                ImageLinks = new [] { "https://link2", "https://link3" },
                RoomsCount = 2,
                ShortDescription = "sh",
                TotalPrice = 10_000_000,
                TransportStops =
                [
                    new ()
                     {
                         Minutes = 10,
                         DistanceType = DistanceType.Foot,
                         Name = "Лесная"
                     }
                ],
                UpdatedAt = new DateTime(2023, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                TotalFloorsNumber = 9,
                Square = 30
            }
        };
        
        await _processor.ProcessAsync(advertisements);

        DbContext.ChangeTracker.Clear();
        var advertisement = Assert.Single(
            DbContext.Advertisements
                .Include(x => x.TransportStops)
                .Include(x => x.LinkedImages));
        
        Assert.Equal("12", advertisement.SourceId);
        Assert.Equal(AdvertisementSource.Cian, advertisement.SourceType);
        Assert.Equal(10_000_000, advertisement.TotalPrice);
        
        Assert.Equal(2, advertisement.TransportStops.Count);
        Assert.Equal(2, advertisement.LinkedImages.Count);
    }
}