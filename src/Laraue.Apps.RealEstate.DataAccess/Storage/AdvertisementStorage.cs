using System.Linq.Expressions;
using Laraue.Apps.RealEstate.Contracts;
using Laraue.Apps.RealEstate.Contracts.Extensions;
using Laraue.Apps.RealEstate.DataAccess.Extensions;
using Laraue.Apps.RealEstate.DataAccess.Models;
using Laraue.Core.DataAccess.Contracts;
using Laraue.Core.DataAccess.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Laraue.Apps.RealEstate.DataAccess.Storage;

public sealed class AdvertisementStorage : IAdvertisementStorage
{
    private readonly AdvertisementsDbContext _dbContext;
    private readonly IHousesStorage _housesStorage;

    public AdvertisementStorage(
        AdvertisementsDbContext dbContext,
        IHousesStorage housesStorage)
    {
        _dbContext = dbContext;
        _housesStorage = housesStorage;
    }
    
    private static readonly Expression<Func<Advertisement, AdvertisementDto>> AdvertisementProjection =
        x => new AdvertisementDto
        {
            Id = x.Id,
            TotalFloorsNumber = x.TotalFloorsNumber,
            Link = x.SourceType.GetAdvertisementUrl(x.Link),
            FloorNumber = x.FloorNumber,
            TotalPrice = x.TotalPrice,
            SquareMeterPrice = Math.Round(x.SquareMeterPrice, 2),
            Square = Math.Round(x.Square, 1),
            RoomsCount = x.RoomsCount,
            SourceId = x.SourceId,
            SourceType = x.SourceType,
            RenovationRating = x.RenovationRating.GetValueOrDefault(),
            ShortDescription = x.ShortDescription,
            Advantages = x.Advantages,
            Problems = x.Problems,
            MetroStations = x.TransportStops
                .Select(y => new AdvertisementMetroStationDto
                {
                    Name = y.TransportStop!.Name,
                    DistanceType = y.DistanceType,
                    DistanceInMinutes = y.DistanceInMinutes,
                    Color = y.TransportStop.Color,
                    Id = y.Id
                }),
            UpdatedAt = x.UpdatedAt,
            FirstTimeCrawledAt = x.FirstTimeCrawledAt,
            CrawledAt = x.CrawledAt,
            HouseNumber = x.House!.Number,
            CityName = x.House!.Street.City.Name,
            HouseId = x.HouseId,
            Address = x.House.Street.Name,
            Images = x.LinkedImages
                .Select(y => new AdvertisementImageDto
                {
                    Url = y.Image.Url,
                })
        };

    public async Task<IShortPaginatedResult<AdvertisementDto>> GetAdvertisementsAsync(
        AdvertisementsRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Advertisements
            .Where(x => x.ReadyAt != null)
            .Where(x => x.RenovationRating > 0)
            .Where(x => x.DeletedAt == null);
        
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

        if (filter.CityId is not null)
        {
            query = query.Where(x => x.House!.Street.CityId == filter.CityId);
        }

        if (filter.MinPrice is not null)
        {
            query = query.Where(x => x.TotalPrice >= filter.MinPrice);
        }
        
        if (filter.MaxPrice is not null)
        {
            query = query.Where(x => x.TotalPrice <= filter.MaxPrice);
        }
        
        if (filter.MaxRenovationRating is not null)
        {
            query = query.Where(x => x.RenovationRating <= filter.MaxRenovationRating);
        }
        
        if (filter.MinRenovationRating is not null)
        {
            query = query.Where(x => x.RenovationRating >= filter.MinRenovationRating);
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
        
        if (!string.IsNullOrEmpty(filter.SearchString))
        {
            query = query.Where(x => EF.Functions.TrigramsAreNotWordSimilar(x.ShortDescription!, filter.SearchString));
        }
        
        var orderedQuery = filter.SortBy switch
        {
            AdvertisementsSort.UpdatedAt => query.OrderBy(x => x.UpdatedAt, filter.SortOrderBy),
            AdvertisementsSort.SquareMeterPrice => query.OrderBy(x => x.SquareMeterPrice, filter.SortOrderBy),
            AdvertisementsSort.RenovationRating => query.OrderBy(x => x.RenovationRating, filter.SortOrderBy),
            AdvertisementsSort.TotalPrice => query.OrderBy(x => x.TotalPrice, filter.SortOrderBy),
            AdvertisementsSort.Square => query.OrderBy(x => x.Square, filter.SortOrderBy),
            AdvertisementsSort.RoomsCount => query.OrderBy(x => x.RoomsCount, filter.SortOrderBy),
            _ => throw new InvalidOperationException("Unknown SortBy"),
        };
        
        orderedQuery = orderedQuery.ThenBy(x => x.UpdatedAt);
        
        var result = await orderedQuery
            .Select(AdvertisementProjection)
            .ShortPaginateEFAsync(request, cancellationToken);

        await EnrichMarketPrice(result.Data, cancellationToken);
        
        return result;
    }
    
    public async Task<AdvertisementDto?> GetAdvertisementByIdAsync(
        AdvertisementByIdRequest request)
    {
        return await _dbContext.Advertisements
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(AdvertisementProjection)
            .SingleOrDefaultAsync();
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

    private async Task EnrichMarketPrice(IList<AdvertisementDto> advertisements, CancellationToken cancellationToken)
    {
        var houseIds = advertisements
            .Select(a => a.HouseId)
            .Where(a => a != null)
            .Cast<long>()
            .ToArray();
        
        var similarAdvertisement = await GetSimilarAdvertisements(houseIds, cancellationToken);
        var similarAdvertisementsByHouseId = similarAdvertisement
            .GroupBy(x => x.HouseId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        foreach (var advertisement in advertisements)
        {
            if (advertisement.HouseId is not null
                && similarAdvertisementsByHouseId.TryGetValue(advertisement.HouseId.Value, out var similarAdvertisements))
            {
                EnrichMarketPrice(advertisement, similarAdvertisements);
            }
        }
    }

    private static void EnrichMarketPrice(AdvertisementDto advertisement, SimilarAdvertisement[] similarAdvertisements)
    {
        const int minAdvRequired = 5;
        
        if (similarAdvertisements.Length < minAdvRequired + 1)
        {
            return;
        }
        
        var similar = similarAdvertisements
            .Where(a => a.Id != advertisement.Id)
            .ToArray();

        var medianSquareMeterPrice = similar
            .Select(s => s.SquareMeterPrice)
            .Median();

        var medianRenovationRating = similar
            .Select(s => s.RenovationRating)
            .Median();

        var medianRenovationUnitSquareMeterPrice = medianSquareMeterPrice / (decimal)medianRenovationRating;
        advertisement.PredictedMarketPrice = medianRenovationUnitSquareMeterPrice * advertisement.RenovationRating * advertisement.Square;
    }

    private async Task<SimilarAdvertisement[]> GetSimilarAdvertisements(IEnumerable<long> houseIds, CancellationToken cancellationToken)
    {
        var houseAddresses = await _dbContext.Houses
            .Where(h => houseIds.Contains(h.Id))
            .Select(h => new
            {
                h.Id,
                h.StreetId,
                HouseNumber = h.Number,
            })
            .ToArrayAsync(cancellationToken);

        var addressesToSearch = new HashSet<SearchableByStreetIdFlatAddress>();
        foreach (var houseAddress in houseAddresses)
        {
            var possibleIntHouseNumber = GetHouseNumber(houseAddress.HouseNumber);
            var searchableHouseNumbers = GetHouseNumberWithNeighbours(possibleIntHouseNumber);

            foreach (var searchableHouseNumber in searchableHouseNumbers)
            {
                addressesToSearch.Add(
                    new SearchableByStreetIdFlatAddress
                    {
                        StreetId = houseAddress.StreetId,
                        HouseNumber = searchableHouseNumber,
                    });
            }
        }

        var allHouses = await _housesStorage.GetHouses(
            addressesToSearch,
            cancellationToken);

        return await _dbContext.Advertisements
            .Where(a => allHouses.Values.Contains(a.HouseId!.Value))
            .Where(a => a.RenovationRating > 0)
            .Select(a => new SimilarAdvertisement
            {
                Id = a.Id,
                HouseNumber = a.House!.Number,
                RenovationRating = a.RenovationRating!.Value,
                Square = a.Square,
                SquareMeterPrice = a.SquareMeterPrice,
                HouseId = a.HouseId!.Value,
            })
            .ToArrayAsync(cancellationToken);
    }

    private string GetHouseNumber(string houseNumber)
    {
        var lastNumberIndex = 0;
        foreach (var houseNumberChar in houseNumber)
        {
            if (houseNumberChar is < '0' or > '9')
            {
                break;
            }

            lastNumberIndex++;
        }

        var intHouseNumber = houseNumber[..lastNumberIndex];
        return intHouseNumber;
    }

    private string[] GetHouseNumberWithNeighbours(string houseNumber)
    {
        if (!int.TryParse(houseNumber, out var houseNumberInt))
        {
            return [houseNumber];
        }
        
        const int maxOffset = 15;

        var result = new List<string>(maxOffset + 1)
        {
            houseNumber
        };

        for (var i = houseNumberInt; i >= houseNumberInt - maxOffset; i--)
        {
            if (i < 1)
            {
                break;
            }
            
            result.Add(i.ToString());
        }
        
        return result.ToArray();
    }

    public record SimilarAdvertisement
    {
        public long Id { get; init; }
        public decimal Square { get; init; }
        public decimal SquareMeterPrice { get; init; }
        public int RenovationRating { get; init; }
        public long HouseId { get; init; }
        public string HouseNumber { get; init; }
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