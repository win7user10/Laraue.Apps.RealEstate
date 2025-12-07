using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Db.Models;
using Laraue.Apps.RealEstate.Db.Storage;
using Laraue.Core.DataAccess.Contracts;
using Xunit;

namespace Laraue.Apps.RealEstate.IntegrationTests.Notifier;

public sealed class AdvertisementStorageTests : TestWithDatabase
{
    private readonly AdvertisementStorage _storage;

    public AdvertisementStorageTests()
    {
        _storage = new AdvertisementStorage(DbContext, new HousesStorage(DbContext));
        
        DbContext.Advertisements.Add(new Advertisement
        {
            UpdatedAt = new DateTime(2022, 01, 01, 0, 0, 1, DateTimeKind.Utc),
            TotalPrice = 500_000,
            SourceId = "12",
            FloorNumber = 1,
            RoomsCount = 2,
            Square = 30,
            TotalFloorsNumber = 5,
            RenovationRating = 5,
            TransportStops = new List<AdvertisementTransportStop>
            {
                new () { DistanceType = DistanceType.Foot, DistanceInMinutes = 6, TransportStopId = 3 },
                new () { DistanceType = DistanceType.Foot, DistanceInMinutes = 10, TransportStopId = 2 },
            },
            ReadyAt = new DateTime(2022, 01, 01, 0, 0, 1, DateTimeKind.Utc),
        });
        
        DbContext.Advertisements.Add(new Advertisement
        {
            UpdatedAt = new DateTime(2022, 01, 02, 0, 0, 1, DateTimeKind.Utc),
            TotalPrice = 600_000,
            SourceId = "13",
            FloorNumber = 3,
            RoomsCount = 2,
            Square = 35,
            TotalFloorsNumber = 5,
            RenovationRating = 6,
            TransportStops = new List<AdvertisementTransportStop>
            {
                new () { DistanceType = DistanceType.Foot, DistanceInMinutes = 12, TransportStopId = 3 },
                new () { DistanceType = DistanceType.Foot, DistanceInMinutes = 13, TransportStopId = 2 },
            },
            ReadyAt = new DateTime(2022, 01, 01, 0, 0, 1, DateTimeKind.Utc),
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public async Task Advertisements_ShouldHasCorrectedPredictedPriceAsync()
    {
        var results = await _storage.GetAdvertisementsAsync(
            new AdvertisementsRequest
            {
                Filter = new Filter()
                {
                    MaxDate = new DateTime(2022, 01, 02, 0, 0, 0, DateTimeKind.Utc),
                    MinDate = new DateTime(2022, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    SortOrderBy = SortOrder.Ascending,
                    SortBy = AdvertisementsSort.UpdatedAt,
                },
                Pagination = new PaginationData
                {
                    PerPage = 5,
                }
            });
        
        Assert.Single(results.Data);
    }
}