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
        
        var filter = request.Filter;
        
        if (filter.MinDate is not null)
        {
            query = query.Where(x => x.UpdatedAt >= filter.MinDate);
        }
        
        if (filter.MaxDate is not null)
        {
            query = query.Where(x => x.UpdatedAt <= filter.MaxDate);
        }
        
        if (filter.ExcludeFirstFloor)
        {
            query = query.Where(x => x.FloorNumber != 1);
        }

        if (filter.ExcludeLastFloor)
        {
            query = query.Where(x => x.FloorNumber != x.TotalFloorsNumber);
        }

        if (filter.MinPrice is not null)
        {
            query = query.Where(x => x.TotalPrice > filter.MinPrice);
        }
        
        if (filter.MaxPrice is not null)
        {
            query = query.Where(x => x.TotalPrice <= filter.MaxPrice);
        }
        
        if (filter.MaxRenovationRating is not null)
        {
            query = query.Where(x => x.RenovationRating < filter.MaxRenovationRating);
        }
        
        if (filter.MinRenovationRating is not null)
        {
            query = query.Where(x => x.RenovationRating > filter.MinRenovationRating);
        }
        
        if (filter.MetroIds?.Any() ?? false)
        {
            query = query
                .Where(adv => adv.TransportStops
                .Any(transportStop => filter.MetroIds
                    .Contains(transportStop.TransportStopId)));
        }
        
        if (filter.MinMetroStationPriority is not null)
        {
            query = query
                .Where(adv => adv.TransportStops
                .Any(transportStop =>
                    transportStop.TransportStop!.Priority
                    > filter.MinMetroStationPriority));
        }

        if (filter.DistanceInMinutes is not null)
        {
            query = query
                .Where(adv => adv.TransportStops
                    .Any(transportStop =>
                        transportStop.DistanceInMinutes
                        <= filter.DistanceInMinutes));
        }
        
        if (filter.MaxPerSquareMeterPrice is not null)
        {
            query = query.Where(x => x.SquareMeterPrice <= filter.MaxPerSquareMeterPrice);
        }
        
        if (filter.MinPerSquareMeterPrice is not null)
        {
            query = query.Where(x => x.SquareMeterPrice >= filter.MinPerSquareMeterPrice);
        }
        
        if (filter.MinSquare is not null)
        {
            query = query.Where(x => x.Square >= filter.MinSquare);
        }
        
        if (filter.RoomsCount?.Any() ?? false)
        {
            query = query.Where(x => filter.RoomsCount.Contains(x.RoomsCount));
        }

        if (filter.Source is not null)
        {
            query = query.Where(x => x.SourceType == filter.Source);
        }
        
        query = filter.SortBy switch
        {
            AdvertisementsSort.UpdatedAt => query.OrderBy(x => x.UpdatedAt, filter.SortOrderBy),
            AdvertisementsSort.SquareMeterPrice => query.OrderBy(x => x.SquareMeterPrice, filter.SortOrderBy),
            AdvertisementsSort.RenovationRating => query.OrderBy(x => x.RenovationRating, filter.SortOrderBy),
            AdvertisementsSort.TotalPrice => query.OrderBy(x => x.TotalPrice, filter.SortOrderBy),
            AdvertisementsSort.Square => query.OrderBy(x => x.Square, filter.SortOrderBy),
            AdvertisementsSort.RealSquareMeterPrice => query.OrderBy(x => x.SquareMeterPredictedPrice, filter.SortOrderBy),
            AdvertisementsSort.RoomsCount => query.OrderBy(x => x.RoomsCount, filter.SortOrderBy),
            AdvertisementsSort.Ideality => query.OrderBy(x => x.Ideality, filter.SortOrderBy),
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
            Images = x.LinkedImages
                .Select(y => new AdvertisementImageDto
                {
                    Url = y.Image.Url,
                    Description = y.Image.Description,
                    RenovationRating = y.Image.RenovationRating,
                    Tags = y.Image.Tags,
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