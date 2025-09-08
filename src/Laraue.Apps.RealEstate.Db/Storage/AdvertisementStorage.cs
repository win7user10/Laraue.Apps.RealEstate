using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Abstractions.Extensions;
using Laraue.Apps.RealEstate.Db.Models;
using Laraue.Core.DataAccess.Contracts;
using Laraue.Core.DataAccess.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Laraue.Apps.RealEstate.Db.Storage;

public sealed class AdvertisementStorage : IAdvertisementStorage
{
    private readonly AdvertisementsDbContext _dbContext;

    public AdvertisementStorage(AdvertisementsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IShortPaginatedResult<AdvertisementDto>> GetAdvertisementsAsync(
        AdvertisementsRequest request)
    {
        var query = _dbContext.Advertisements
            .Where(x => x.RenovationRating != null);
        
        if (request.MinDate is not null)
        {
            query = query.Where(x => x.UpdatedAt >= request.MinDate);
        }
        
        if (request.MaxDate is not null)
        {
            query = query.Where(x => x.UpdatedAt <= request.MaxDate);
        }
        
        if (request.ExcludeFirstFloor)
        {
            query = query.Where(x => x.FloorNumber != 1);
        }

        if (request.ExcludeLastFloor)
        {
            query = query.Where(x => x.FloorNumber != x.TotalFloorsNumber);
        }

        if (request.MinPrice is not null)
        {
            query = query.Where(x => x.TotalPrice > request.MinPrice);
        }
        
        if (request.MaxPrice is not null)
        {
            query = query.Where(x => x.TotalPrice <= request.MaxPrice);
        }
        
        if (request.MaxRenovationRating is not null)
        {
            query = query.Where(x => x.RenovationRating < request.MaxRenovationRating);
        }
        
        if (request.MinRenovationRating is not null)
        {
            query = query.Where(x => x.RenovationRating > request.MinRenovationRating);
        }
        
        if (request.MetroIds?.Any() ?? false)
        {
            query = query
                .Where(adv => adv.TransportStops
                .Any(transportStop => request.MetroIds
                    .Contains(transportStop.TransportStopId)));
        }
        
        if (request.MinMetroStationPriority is not null)
        {
            query = query
                .Where(adv => adv.TransportStops
                .Any(transportStop =>
                    transportStop.TransportStop!.Priority
                    > request.MinMetroStationPriority));
        }

        if (request.DistanceInMinutes is not null)
        {
            query = query
                .Where(adv => adv.TransportStops
                    .Any(transportStop =>
                        transportStop.DistanceInMinutes
                        <= request.DistanceInMinutes));
        }
        
        if (request.MaxPerSquareMeterPrice is not null)
        {
            query = query.Where(x => x.SquareMeterPrice <= request.MaxPerSquareMeterPrice);
        }
        
        if (request.MinPerSquareMeterPrice is not null)
        {
            query = query.Where(x => x.SquareMeterPrice >= request.MinPerSquareMeterPrice);
        }
        
        if (request.MinSquare is not null)
        {
            query = query.Where(x => x.Square >= request.MinSquare);
        }
        
        if (request.RoomsCount?.Any() ?? false)
        {
            query = query.Where(x => request.RoomsCount.Contains(x.RoomsCount));
        }
        
        query = request.SortBy switch
        {
            AdvertisementsSort.UpdatedAt => query.OrderBy(x => x.UpdatedAt, request.SortOrderBy),
            AdvertisementsSort.SquareMeterPrice => query.OrderBy(x => x.SquareMeterPrice, request.SortOrderBy),
            AdvertisementsSort.RenovationRating => query.OrderBy(x => x.RenovationRating, request.SortOrderBy),
            AdvertisementsSort.TotalPrice => query.OrderBy(x => x.TotalPrice, request.SortOrderBy),
            AdvertisementsSort.Square => query.OrderBy(x => x.Square, request.SortOrderBy),
            AdvertisementsSort.RealSquareMeterPrice => query.OrderBy(x => x.SquareMeterPredictedPrice, request.SortOrderBy),
            AdvertisementsSort.RoomsCount => query.OrderBy(x => x.RoomsCount, request.SortOrderBy),
            AdvertisementsSort.Ideality => query.OrderBy(x => x.Ideality, request.SortOrderBy),
            _ => throw new Exception(),
        };
        
        return await query.Select(x => new AdvertisementDto
        {
            TotalFloorsNumber = x.TotalFloorsNumber,
            Link = x.SourceType.GetAdvertisementUrl(x.Link),
            FloorNumber = x.FloorNumber,
            TotalPrice = x.TotalPrice,
            SquareMeterPrice = Math.Round(x.SquareMeterPrice, 2),
            Square = Math.Round(x.Square, 1),
            RoomsCount = x.RoomsCount,
            SourceId = x.SourceId,
            SourceType = x.SourceType,
            RenovationRating = Math.Round(x.RenovationRating.GetValueOrDefault(), 2),
            Ideality = Math.Round(x.Ideality, 2),
            ShortDescription = x.ShortDescription,
            MetroStations = x.TransportStops
                .Select(y => new AdvertisementMetroStationDto
                {
                    Name = y.TransportStop!.Name,
                    DistanceType = y.DistanceType,
                    DistanceInMinutes = y.DistanceInMinutes,
                    Color = y.TransportStop.Color,
                    Id = y.Id
                }),
            RealSquareMeterPrice = Math.Round(x.SquareMeterPredictedPrice, 2),
            UpdatedAt = x.UpdatedAt,
            Images = x.Images
                .Select(y => new AdvertisementImageDto
                {
                    Url = y.Url,
                    Description = y.Decription,
                    RenovationRating = y.RenovationRating,
                    Tags = y.Tags,
                })
        }).ShortPaginateEFAsync(request);
    }

    public async Task<IList<MainChartDayItemDto>> GetMainChartAsync(RangeChartRequest request)
    {
        var query = _dbContext
            .Advertisements
            .Where(x => x.UpdatedAt >= request.MinDate && x.UpdatedAt <= request.MaxDate);

        return await GetMainChartAsync(query);
    }

    public async Task<IList<MainChartDayItemDto>> GetMainChartAsync(IEnumerable<DateTime> dates)
    {
        dates = dates.Select(x => x.Date);
        
        var query = _dbContext
            .Advertisements
            .Where(x => dates.Contains(x.UpdatedAt.Date));

        return await GetMainChartAsync(query);
    }

    private static Task<List<MainChartDayItemDto>> GetMainChartAsync(IQueryable<Advertisement> query)
    {
        return query.GroupBy(x => x.UpdatedAt.Date)
            .Select(x => new MainChartDayItemDto
            {
                Date = x.Key,
                Data = x.GroupBy(y => y.SourceType)
                    .Select(y => new MainChartItemDayItemDataDto
                    {
                        AdvertisementCount = y.Count(),
                        AveragePrice = (int)y.Average(z => z.TotalPrice),
                        AverageSquareMeterPrice = (int)y.Average(z => z.SquareMeterPrice),
                        SourceType = y.Key,
                    })
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
    }
}