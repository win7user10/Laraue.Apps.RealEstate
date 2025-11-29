using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Db.Models;
using Laraue.Apps.RealEstate.Db.Storage;
using Laraue.Apps.RealEstate.Telegram;
using Laraue.Core.DateTime.Services.Abstractions;
using Moq;
using Xunit;

namespace Laraue.Apps.RealEstate.IntegrationTests.Notifier;

public sealed class AdvertisementsPickerTests : TestWithDatabase
{
    private readonly PublicAdvertisementsPicker _picker;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new ();

    public AdvertisementsPickerTests()
    {
        _picker = new PublicAdvertisementsPicker(
            DbContext,
            _dateTimeProviderMock.Object,
            new AdvertisementStorage(DbContext, new HousesStorage(DbContext)));
    }

    [Fact]
    public async Task Advertisements_ShouldBeCorrectlyTaken_WhenSessionIdIsNullAsync()
    {
        DbContext.Advertisements.Add(new Db.Models.Advertisement
        {
            UpdatedAt = new DateTime(2022, 01, 01, 0, 0, 0, DateTimeKind.Utc),
            TotalPrice = 5_500_000,
            SourceId = "12",
            FloorNumber = 1,
            RoomsCount = 2,
            Square = 30,
            TotalFloorsNumber = 5,
            RenovationRating = 1,
            TransportStops = new List<AdvertisementTransportStop>
            {
                new () { DistanceType = DistanceType.Foot, DistanceInMinutes = 6, TransportStopId = 3 },
                new () { DistanceType = DistanceType.Foot, DistanceInMinutes = 10, TransportStopId = 2 },
            }
        });
        
        DbContext.Advertisements.Add(new Db.Models.Advertisement
        {
            UpdatedAt = new DateTime(2022, 01, 02, 0, 0, 0, DateTimeKind.Utc),
            TotalPrice = 5_600_000,
            SourceId = "13",
            FloorNumber = 3,
            RoomsCount = 2,
            Square = 35,
            TotalFloorsNumber = 5,
            RenovationRating = 1,
            TransportStops = new List<AdvertisementTransportStop>
            {
                new () { DistanceType = DistanceType.Foot, DistanceInMinutes = 12, TransportStopId = 3 },
                new () { DistanceType = DistanceType.Foot, DistanceInMinutes = 13, TransportStopId = 2 },
            }
        });

        var sessionId = 1;
        DbContext.CrawlingSessions.Add(new CrawlingSession
        {
            Id = sessionId,
            StartedAt = new DateTime(2022, 01, 01, 0, 00, 0, DateTimeKind.Utc),
            FinishedAt = new DateTime(2022, 01, 01, 0, 20, 0, DateTimeKind.Utc),
        });

        await DbContext.SaveChangesAsync();
        
        var newAdvertisements = await _picker.GetBestSinceSessionAsync(null);
        
        Assert.Equal(sessionId, newAdvertisements.LastSessionId);
        Assert.Single(newAdvertisements.Advertisements);
    }
}