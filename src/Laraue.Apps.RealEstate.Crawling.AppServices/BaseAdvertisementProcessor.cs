using System.Diagnostics.CodeAnalysis;
using Laraue.Apps.RealEstate.AppServices.TransportStops;
using Laraue.Apps.RealEstate.Contracts;
using Laraue.Apps.RealEstate.Crawling.Contracts;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.DataAccess.Models;
using Laraue.Apps.RealEstate.DataAccess.Storage;
using Laraue.Apps.RealEstate.Db;
using Laraue.Core.DataAccess.EFCore.Extensions;
using Laraue.Core.DateTime.Services.Abstractions;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Advertisement = Laraue.Apps.RealEstate.Crawling.Contracts.Advertisement;

namespace Laraue.Apps.RealEstate.Crawling.AppServices;

public abstract class BaseAdvertisementProcessor<TExternalIdentifier> : IAdvertisementProcessor
    where TExternalIdentifier : struct
{
    public AdvertisementSource Source { get; }
    private readonly AdvertisementsDbContext _dbContext;
    private readonly IMetroStationsStorage _metroStationsStorage;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IHousesStorage _housesStorage;
    private readonly ILogger<BaseAdvertisementProcessor<TExternalIdentifier>> _logger;

    private IDictionary<string, MetroStationData>? _externalPublicStopsIds;
    private IDictionary<long, MetroStationData>? _systemPublicStopsIds;

    protected BaseAdvertisementProcessor(
        AdvertisementSource source,
        AdvertisementsDbContext dbContext,
        IMetroStationsStorage metroStationsStorage,
        IDateTimeProvider dateTimeProvider,
        ILogger<BaseAdvertisementProcessor<TExternalIdentifier>> logger,
        IHousesStorage housesStorage)
    {
        Source = source;
        _dbContext = dbContext;
        _metroStationsStorage = metroStationsStorage;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _housesStorage = housesStorage;
    }
    
    public async Task<ProcessResult> ProcessAsync(
        Advertisement[] advertisements,
        long cityId,
        CancellationToken ct = default)
    {
        // Step 1 - update dictionaries
        var addressesTransaction = await _dbContext.BeginTransactionIfNotStartedAsync();
        
        var syncAddressesDictionaryResult = await SyncAddressesDictionaryAsync(advertisements, cityId, ct);
        
        await addressesTransaction.CommitAsync(ct);
        
        // Step 2 - update advertisements
        await using var transaction = await _dbContext.BeginTransactionIfNotStartedAsync();
        
        var processResult = await UpdateAdvertisementsAsync(advertisements, ct);
        
        await UpdateImageLinksAsync(processResult.UpdatedAdvertisements, ct);
        
        await UpdateAdvertisementAddressesAsync(processResult.UpdatedAdvertisements, syncAddressesDictionaryResult, ct);
        
        await UpdateTransportStopsAsync(processResult.UpdatedAdvertisements, ct);
        
        await transaction.CommitAsync(ct);

        return processResult;
    }

    private async Task UpdateAdvertisementAddressesAsync(
        Dictionary<long, Advertisement> advertisements,
        Dictionary<SearchableByStreetNameFlatAddress, long> syncAddressesDictionaryResult,
        CancellationToken cancellationToken = default)
    {
        foreach (var advertisement in advertisements)
        {
            if (advertisement.Value.FlatAddress == null)
            {
                continue;
            }

            var localFlatAddress = ToLocalFlatAddress(advertisement.Value.FlatAddress);
            if (!syncAddressesDictionaryResult.TryGetValue(localFlatAddress, out var houseId))
            {
                continue;
            }

            _logger.LogDebug("Set houseId='{AddressId}' for adv='{AdvId}'", houseId, advertisement.Key);
            
            await _dbContext.Advertisements
                .Where(a => a.Id == advertisement.Key)
                .ExecuteUpdateAsync(update => update
                        .SetProperty(p => p.HouseId, houseId),
                    cancellationToken);
        }
    }

    private static SearchableByStreetNameFlatAddress ToLocalFlatAddress(FlatAddress flatAddress)
    {
        return new SearchableByStreetNameFlatAddress
        {
            Street = AddressNormalizer.NormalizeStreet(flatAddress.Street),
            HouseNumber = AddressNormalizer.NormalizeHouseNumber(flatAddress.HouseNumber),
        };
    }
    
    private async Task<Dictionary<SearchableByStreetNameFlatAddress, long>> SyncAddressesDictionaryAsync(
        Advertisement[] advertisements,
        long cityId,
        CancellationToken ct = default)
    {
        await _dbContext.ShareLockAsync<Street>(ct);
        await _dbContext.ShareLockAsync<House>(ct);
        
        var allAddresses = advertisements
            .Where(x => x.FlatAddress != null)
            .Select(x => ToLocalFlatAddress(x.FlatAddress!))
            .ToHashSet();

        var allStreets = allAddresses
            .Select(x => x.Street)
            .ToHashSet();

        var existsInDbStreets = await _dbContext.Streets
            .Where(s => allStreets.Contains(s.Name))
            .Where(s => s.CityId == cityId)
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsyncEF(c => c.Name, c => c.Id, ct);

        var streetsToInsertIntoDb = allStreets
            .Except(existsInDbStreets.Keys)
            .Select(x => new Street { CityId = cityId, Name = x })
            .ToArray();

        if (streetsToInsertIntoDb.Length > 0)
        {
            await _dbContext.GetTable<Street>()
                .Merge()
                .Using(streetsToInsertIntoDb)
                .On((x, y) => x.Name == y.Name && x.CityId == y.CityId)
                .InsertWhenNotMatched()
                .MergeAsync(ct);
            
            var newStreets = await _dbContext.Streets
                .Where(s => s.CityId == cityId)
                .Where(s => streetsToInsertIntoDb.Select(x => x.Name).Contains(s.Name))
                .Select(s => new { s.Id, s.Name })
                .ToArrayAsyncEF(ct);

            foreach (var newStreet in newStreets)
            {
                existsInDbStreets.Add(newStreet.Name, newStreet.Id);
            }
            
            _logger.LogInformation("Inserted new streets '{Streets}'", string.Join(",", newStreets.Select(s => s.Name)));
        }
        
        var existsInDbHouses = await _housesStorage.GetHouses(cityId, allAddresses, ct);
        
        var housesToInsertIntoDb = allAddresses
            .Where(x => !existsInDbHouses.ContainsKey(x))
            .ToHashSet();

        if (housesToInsertIntoDb.Count > 0)
        {
            var entities = housesToInsertIntoDb
                .Select(x => new House
                {
                    Number = x.HouseNumber,
                    NumberNormalized = AddressNormalizer.NormalizeForSearch(x.HouseNumber),
                    StreetId = existsInDbStreets[x.Street]
                })
                .ToArray();
            
            await _dbContext.GetTable<House>()
                .Merge()
                .Using(entities)
                .On((x, y) => x.StreetId == y.StreetId && x.Number == y.Number)
                .InsertWhenNotMatched()
                .MergeAsync(ct);
            
            var inserted = await _housesStorage.GetHouses(cityId, housesToInsertIntoDb, ct);
            foreach (var insertedHouse in inserted)
            {
                existsInDbHouses.Add(insertedHouse.Key, insertedHouse.Value);
            }
            
            _logger.LogInformation(
                "Inserted new houses '{Streets}'",
                string.Join(",", inserted.Select(s => $"{s.Key.Street} {s.Key.HouseNumber}")));
        }

        return existsInDbHouses;
    }

    private async Task UpdateTransportStopsAsync(
        IDictionary<long, Advertisement> advertisements,
        CancellationToken ct = default)
    {
        var existsTransportStopBySourceId = await GetExistsTransportStopIdsAsync(
            advertisements.Values.Select(x => x.Id),
            ct);
        
        foreach (var advertisement in advertisements)
        {
            // upd transport stops via add new logic
            existsTransportStopBySourceId.TryGetValue(advertisement.Value.Id, out var existsStops);
            existsStops ??= [];
            foreach (var transportStop in advertisement.Value.TransportStops ?? [])
            {
                // Metro stations are different 
                if (!TryGetPublicStopByName(transportStop.Name, out var metroStationData))
                {
                    _logger.LogWarning("Public stop {Name} is not found", transportStop.Name);
                
                    continue;
                }
            
                if (existsStops.Contains(metroStationData.Id))
                {
                    continue;
                }
                
                _dbContext.Add(new AdvertisementTransportStop
                {
                    DistanceType = transportStop.DistanceType,
                    DistanceInMinutes = transportStop.Minutes,
                    TransportStopId = metroStationData.Id,
                    AdvertisementId = advertisement.Key
                });
            }
        }
        
        await _dbContext.SaveChangesAsync(ct);
    }

    private async Task UpdateImageLinksAsync(
        IDictionary<long, Advertisement> advertisements,
        CancellationToken ct = default)
    {
        var imageIdByUrl = await UpsertImagesAsync(advertisements.Values, ct);
        var existsAdvertisementsImageLinks = await GetExistsLinksAsync(
            advertisements.Values.Select(x => x.Id),
            ct);
        
        // upd images via sync to actual state logic
        foreach (var advertisement in advertisements)
        {
            existsAdvertisementsImageLinks.TryGetValue(advertisement.Value.Id, out var existsLinks);
            existsLinks ??= [];
            
            // Remove links that no more exists in advertisement
            foreach (var oldLink in existsLinks)
            {
                if (advertisement.Value.ImageLinks.Contains(oldLink.Url))
                {
                    continue;
                }
                
                var imageToRemove = new AdvertisementImage
                {
                    AdvertisementId = oldLink.AdvertisementId,
                    ImageId = oldLink.ImageId
                };
                
                _dbContext.Remove(imageToRemove);
            }
            
            var existsLinkUrls = existsLinks.Select(x => x.Url).ToArray();
            var newImageUrls = advertisement.Value.ImageLinks.Except(existsLinkUrls).ToHashSet();

            foreach (var newLinkUrl in newImageUrls)
            {
                if (imageIdByUrl.TryGetValue(newLinkUrl, out var imageId))
                {
                    _dbContext.AdvertisementImages.Add(new AdvertisementImage
                    {
                        ImageId = imageId,
                        AdvertisementId = advertisement.Key,
                    });
                }
            }
        }

        await _dbContext.SaveChangesAsync(ct);
    }
    
    private async Task<ProcessResult> UpdateAdvertisementsAsync(
        Advertisement[] advertisements,
        CancellationToken ct = default)
    {
        var existsAdvertisements = await GetExistsAdvertisementsAsync(advertisements, ct);
        var newItemsCount = 0;
        var updatedItemsCount = 0;
        var outdatedItemsIds = new HashSet<long>();
        
        var processedItems = new List<DataAccess.Models.Advertisement>();
        foreach (var parsedPage in advertisements)
        {
            if (parsedPage.Square == 0
                || parsedPage.TotalPrice == 0
                || !parsedPage.UpdatedAt.HasValue
                || parsedPage.UpdatedAt > _dateTimeProvider.UtcNow)
            {
                _logger.LogWarning("Skip adv {Result} due to wrong bindings", parsedPage);
                
                continue;
            }

            var dbAdvertisement = new DataAccess.Models.Advertisement();
            
            if (!existsAdvertisements.TryGetValue(parsedPage.Id, out var advertisement))
            {
                _dbContext.Add(dbAdvertisement);
                
                FillAdvertisement(dbAdvertisement, parsedPage);
                
                // Fill the fields should be setup only on first insert.
                dbAdvertisement.FirstTimeCrawledAt = dbAdvertisement.CrawledAt;
                
                newItemsCount++;
            }
            else
            {
                if (advertisement.UpdatedAt >= parsedPage.UpdatedAt)
                {
                    outdatedItemsIds.Add(advertisement.Id);
                    
                    continue;
                }
                
                // Fill the fields shouldn't be changed.
                dbAdvertisement.Id = advertisement.Id;
                dbAdvertisement.FirstTimeCrawledAt = advertisement.FirstTimeCrawledAt;
                
                _dbContext.Attach(dbAdvertisement);
                FillAdvertisement(dbAdvertisement, parsedPage);

                updatedItemsCount++;
            }

            processedItems.Add(dbAdvertisement);
        }
        
        await _dbContext.SaveChangesAsync(ct);
        
        _logger.LogInformation(
            "Crawling Result: Inserted ({InsertedCount}), Updated ({UpdatedCount}), Outdated ({OutdatedCount}) items.",
            newItemsCount,
            updatedItemsCount,
            outdatedItemsIds.Count);

        var processedItemsDictionary = processedItems
            .Join(
                advertisements,
                x => x.SourceId,
                x => x.Id,
                (model, parsed) => (model, parsed))
            .ToDictionary(x => x.model.Id, x => x.parsed);

        return new ProcessResult
        {
            UpdatedAdvertisements = processedItemsDictionary,
            OutdatedItemsIds = outdatedItemsIds,
        };
    }

    private void FillAdvertisement(
        DataAccess.Models.Advertisement model,
        Advertisement parsedAdvertisement)
    {
        // Upd common fields
        model.Link = parsedAdvertisement.Link;
        model.SourceType = Source;
        model.Square = parsedAdvertisement.Square;
        model.SourceId = parsedAdvertisement.Id;
        model.FloorNumber = parsedAdvertisement.FloorNumber;
        model.RoomsCount = parsedAdvertisement.RoomsCount;
        model.TotalFloorsNumber = parsedAdvertisement.TotalFloorsNumber;
        model.ShortDescription = parsedAdvertisement.ShortDescription;
        model.TotalPrice = parsedAdvertisement.TotalPrice;
        model.UpdatedAt = parsedAdvertisement.UpdatedAt.GetValueOrDefault();
        model.CrawledAt = _dateTimeProvider.UtcNow;
        model.FlatType = parsedAdvertisement.FlatType;
        model.PredictedAt = null;
        model.ReadyAt = null;
    }

    private bool TryGetPublicStopByName(
        string name,
        [MaybeNullWhen(false)]out MetroStationData publicStop)
    {
        UpdateMetroCacheIfRequired();
        
        return _externalPublicStopsIds!.TryGetValue(name, out publicStop);
    }
    
    private MetroStationData GetPublicStopCached(long systemIdentifier)
    {
        UpdateMetroCacheIfRequired();
        
        return _systemPublicStopsIds![systemIdentifier];
    }

    private void UpdateMetroCacheIfRequired()
    {
        if (_externalPublicStopsIds is not null)
        {
            return;
        }
        
        var publicStops = GetPublicStops();
        _externalPublicStopsIds ??= publicStops.ToDictionary(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
        _systemPublicStopsIds ??= publicStops.ToDictionary(x => x.Id);
    }
    
    private MetroStationData[] GetPublicStops()
    {
        return _metroStationsStorage.GetMetroStations();
    }

    protected abstract TExternalIdentifier ParseIdentifier(string stringIdentifier);

    private Task<Dictionary<string, AdvertisementDto>> GetExistsAdvertisementsAsync(
        Advertisement[] advertisements,
        CancellationToken ct = default)
    {
        return _dbContext.Advertisements
            .Where(x => x.SourceType == Source &&
                advertisements
                    .Select(y => y.Id)
                    .Contains(x.SourceId))
            .Select(x => new AdvertisementDto
            {
                Id = x.Id,
                SourceId = x.SourceId,
                UpdatedAt = x.UpdatedAt,
                FirstTimeCrawledAt = x.FirstTimeCrawledAt,
            })
            .ToDictionaryAsyncEF(x => x.SourceId, ct);
    }
    
    private Task<Dictionary<string, ImageLinkDto[]>> GetExistsLinksAsync(
        IEnumerable<string> sourceIds,
        CancellationToken ct = default)
    {
        return _dbContext.Advertisements
            .Where(x => x.SourceType == Source && sourceIds.Contains(x.SourceId))
            .Select(x => new
            {
                x.SourceId,
                LinkedImages = x.LinkedImages.Select(y => new ImageLinkDto
                {
                    AdvertisementId = y.AdvertisementId,
                    ImageId = y.ImageId,
                    Url = y.Image.Url,
                })
                .ToArray(),
            })
            .ToDictionaryAsyncEF(x => x.SourceId, x => x.LinkedImages, ct);
    }
    
    private Task<Dictionary<string, long[]>> GetExistsTransportStopIdsAsync(
        IEnumerable<string> sourceIds,
        CancellationToken ct = default)
    {
        return _dbContext.Advertisements
            .Where(x => x.SourceType == Source && sourceIds.Contains(x.SourceId))
            .Select(x => new
            {
                x.SourceId,
                TransportStopIds = x.TransportStops.Select(y => y.TransportStopId).ToArray()
            })
            .ToDictionaryAsyncEF(x => x.SourceId, x => x.TransportStopIds,  ct);
    }

    public class AdvertisementDto
    {
        public required long Id { get; set; }
        public required string SourceId { get; set; }
        public required DateTime UpdatedAt { get; set; }
        public required DateTime FirstTimeCrawledAt { get; set; }
    }

    public class ImageLinkDto
    {
        public required string Url { get; init; }
        public required long ImageId { get; init; }
        public required long AdvertisementId { get; init; }
    }
    
    private async Task<IDictionary<string, long>> UpsertImagesAsync(
        IEnumerable<Advertisement> advertisements,
        CancellationToken ct = default)
    {
        var allAdvertisementImagesUrls = advertisements
            .SelectMany(y => y.ImageLinks)
            .Where(y => Uri.TryCreate(y, UriKind.Absolute, out _))
            .ToArray();
        
        var existUrls = await _dbContext.Images
            .Where(x => allAdvertisementImagesUrls.Contains(x.Url))
            .ToDictionaryAsyncEF(x => x.Url, x => x.Id, ct);

        var notExistUrls = allAdvertisementImagesUrls
            .Except(existUrls.Keys);

        var lastDate = _dateTimeProvider.UtcNow;
        
        var newImages = notExistUrls
            .Select(notExistUrl => new Image { Url = notExistUrl, LastAvailableAt = lastDate })
            .ToList();

        _dbContext.Images.AddRange(newImages);
        await _dbContext.SaveChangesAsync(ct);

        foreach (var newImage in newImages)
        {
            existUrls.Add(newImage.Url, newImage.Id);
        }

        return existUrls;
    }
}