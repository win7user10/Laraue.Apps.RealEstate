using Laraue.Apps.RealEstate.DataAccess.Models;
using LinqToDB.EntityFrameworkCore;

namespace Laraue.Apps.RealEstate.DataAccess.Storage;

public interface IHousesStorage
{
    Task<Dictionary<SearchableByStreetNameFlatAddress, long>> GetHouses(
        long cityId,
        IEnumerable<SearchableByStreetNameFlatAddress> houses,
        CancellationToken ct = default);
    
    Task<Dictionary<SearchableByStreetIdFlatAddress, long>> GetHouses(
        IEnumerable<SearchableByStreetIdFlatAddress> houses,
        CancellationToken ct = default);
}

public class HousesService(AdvertisementsDbContext dbContext) : IHousesStorage
{
    public async Task<Dictionary<SearchableByStreetNameFlatAddress, long>> GetHouses(
        long cityId,
        IEnumerable<SearchableByStreetNameFlatAddress> houses,
        CancellationToken ct = default)
    {
        var housesHashSet = houses.ToHashSet();
        
        if (housesHashSet.Count == 0)
        {
            return new Dictionary<SearchableByStreetNameFlatAddress, long>();
        }
        
        IQueryable<QueryableHouse>? query = null;
        foreach (var house in housesHashSet)
        {
            var innerQuery = dbContext.Houses
                .Where(h => h.Number == house.HouseNumber)
                .Where(h => h.Street.Name == house.Street)
                .Where(h => h.Street.CityId == cityId)
                .Select(x => new QueryableHouse(x.Street.Name, x.Number, x.Id));
            
            query = query is null ? innerQuery : query.Concat(innerQuery);
        }

        return await query!
            .ToDictionaryAsyncLinqToDB(
                x => new SearchableByStreetNameFlatAddress
                {
                    Street = x.StreetName,
                    HouseNumber = x.HouseNumber,
                },
                x => x.HouseId,
                ct);
    }

    public async Task<Dictionary<SearchableByStreetIdFlatAddress, long>> GetHouses(
        IEnumerable<SearchableByStreetIdFlatAddress> houses,
        CancellationToken ct = default)
    {
        var housesHashSet = houses.ToHashSet();
        if (housesHashSet.Count == 0)
        {
            return new Dictionary<SearchableByStreetIdFlatAddress, long>();
        }

        var grouped = housesHashSet
            .ToLookup(x => x.StreetId, x => x.HouseNumber);
        
        IQueryable<House>? query = null;
        foreach (var byStreet in grouped)
        {
            var streetQuery = dbContext.Houses
                .Where(h => h.StreetId == byStreet.Key
                    && h.NumberNormalized.Any(n => byStreet.Any(s => n == s)));
            
            query = query is null ? streetQuery : query.Concat(streetQuery);
        }
        
        var result = await query!
            .Select(x => new 
            {
                x.Id,
                x.StreetId,
                x.Number,
            })
            .ToArrayAsyncEF(ct);
        
        return result.ToDictionary(
            x => new SearchableByStreetIdFlatAddress
            {
                HouseNumber = x.Number,
                StreetId = x.StreetId,
            },
            x => x.Id);
    }

    private record QueryableHouse(string StreetName, string HouseNumber, long HouseId);
}

public record SearchableByStreetNameFlatAddress
{
    public required string Street { get; init; }
    public required string HouseNumber { get; init; }
}

public record SearchableByStreetIdFlatAddress
{
    public required long StreetId { get; init; }
    public required string HouseNumber { get; init; }
}