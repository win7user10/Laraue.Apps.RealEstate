using System.Diagnostics.CodeAnalysis;
using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.TransportStops;
using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.Db.Models;
using Laraue.Core.DateTime.Services.Abstractions;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Advertisement = Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts.Advertisement;

namespace Laraue.Apps.RealEstate.Crawling.Impl;

public abstract class BaseAdvertisementProcessor<TExternalIdentifier> : IAdvertisementProcessor
    where TExternalIdentifier : struct
{
    public AdvertisementSource Source { get; }
    private readonly AdvertisementsDbContext _dbContext;
    private readonly IMetroStationsStorage _metroStationsStorage;
    private readonly IDateTimeProvider _dateTimeProvider;

    private IDictionary<string, MetroStationData>? _externalPublicStopsIds;
    private IDictionary<long, MetroStationData>? _systemPublicStopsIds;

    protected BaseAdvertisementProcessor(
        AdvertisementSource source,
        AdvertisementsDbContext dbContext,
        IMetroStationsStorage metroStationsStorage,
        IDateTimeProvider dateTimeProvider)
    {
        Source = source;
        _dbContext = dbContext;
        _metroStationsStorage = metroStationsStorage;
        _dateTimeProvider = dateTimeProvider;
    }
    
    public async Task<HashSet<long>> ProcessAsync(
        Advertisement[] advertisements,
        CancellationToken ct = default)
    {
        var existsAdvertisements = await GetExistsAdvertisementsAsync(advertisements, ct);

        var imageIdByUrl = await UpsertImagesAsync(advertisements, ct);
        
        var updatedAdvertisements = new List<Db.Models.Advertisement>();
        foreach (var parsedPage in advertisements)
        {
            if (parsedPage.Square == 0 || parsedPage.TotalPrice == 0 || parsedPage.UpdatedAt > DateTime.UtcNow)
            {
                // Something wrong with binding
                
                continue;
            }
            
            if (!existsAdvertisements.TryGetValue(ParseIdentifier(parsedPage.Id), out var advertisement))
            {
                advertisement = new Db.Models.Advertisement();
                _dbContext.Add(advertisement);
                
                UpdateAdvertisement(advertisement, parsedPage, imageIdByUrl);
            }
            else
            {
                if (advertisement.UpdatedAt >= parsedPage.UpdatedAt)
                {
                    continue;
                }
                
                UpdateAdvertisement(advertisement, parsedPage, imageIdByUrl);
            }

            updatedAdvertisements.Add(advertisement);
        }
        
        await _dbContext.SaveChangesAsync(ct);
        
        return updatedAdvertisements.Select(x => x.Id).ToHashSet();
    }

    private void UpdateAdvertisement(
        Db.Models.Advertisement dbAdvertisement,
        Advertisement parsedAdvertisement,
        IDictionary<string, long> imageIdByUrl)
    {
        // Upd common fields
        dbAdvertisement.Link = parsedAdvertisement.Link;
        dbAdvertisement.SourceType = Source;
        dbAdvertisement.Square = parsedAdvertisement.Square;
        dbAdvertisement.SourceId = parsedAdvertisement.Id;
        dbAdvertisement.FloorNumber = parsedAdvertisement.FloorNumber;
        dbAdvertisement.RoomsCount = parsedAdvertisement.RoomsCount;
        dbAdvertisement.TotalFloorsNumber = parsedAdvertisement.TotalFloorsNumber;
        dbAdvertisement.ShortDescription = parsedAdvertisement.ShortDescription;
        dbAdvertisement.TotalPrice = parsedAdvertisement.TotalPrice;
        dbAdvertisement.UpdatedAt = parsedAdvertisement.UpdatedAt.GetValueOrDefault();
        dbAdvertisement.CrawledAt = _dateTimeProvider.UtcNow;
        dbAdvertisement.FlatType = parsedAdvertisement.FlatType;
        dbAdvertisement.PredictedAt = null;
        
        // upd images via sync to actual state logic
        foreach (var removedAdvertisementImage in dbAdvertisement.LinkedImages
            .Where(x => !parsedAdvertisement.ImageLinks.Contains(x.Image.Url)))
        {
            _dbContext.Remove(removedAdvertisementImage);
        }

        var existImagesUrls = dbAdvertisement.LinkedImages.Select(y => y.Image.Url);
        var newImageUrls = parsedAdvertisement.ImageLinks.Except(existImagesUrls).ToHashSet();

        foreach (var newLink in newImageUrls)
        {
            if (imageIdByUrl.TryGetValue(newLink, out var imageId))
            {
                dbAdvertisement.LinkedImages.Add(new AdvertisementImage
                {
                    ImageId = imageId
                });
            }
        }
        
        // upd transport stops via add new logic
        var existStops = dbAdvertisement.TransportStops
            .Select(x => x.TransportStopId)
            .ToHashSet();
        
        foreach (var transportStop in parsedAdvertisement.TransportStops ?? [])
        {
            // Metro stations are different 
            if (!TryGetPublicStopByName(transportStop.Name, out var metroStationData))
            {
                continue;
            }
            
            if (existStops.Contains(metroStationData.Id))
            {
                continue;
            }
                
            dbAdvertisement.TransportStops.Add(new AdvertisementTransportStop
            {
                DistanceType = transportStop.DistanceType,
                DistanceInMinutes = transportStop.Minutes,
                TransportStopId = metroStationData.Id,
            });
        }
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
        _externalPublicStopsIds ??= publicStops.ToDictionary(x => x.Name);
        _systemPublicStopsIds ??= publicStops.ToDictionary(x => x.Id);
    }
    
    
    private MetroStationData[] GetPublicStops()
    {
        return _metroStationsStorage.GetMetroStations();
    }

    protected abstract TExternalIdentifier ParseIdentifier(string stringIdentifier);

    private Task<Dictionary<TExternalIdentifier, Laraue.Apps.RealEstate.Db.Models.Advertisement>> GetExistsAdvertisementsAsync(
        Advertisement[] advertisements,
        CancellationToken ct = default)
    {
        return _dbContext.Advertisements
            .Include(x => x.LinkedImages)
            .ThenInclude(x => x.Image)
            .Include(x => x.TransportStops)
            .ThenInclude(x => x.TransportStop)
            .Where(x => x.SourceType == Source && 
                advertisements
                    .Select(y => y.Id.ToString())
                    .Contains(x.SourceId))
            .ToDictionaryAsyncEF(x => ParseIdentifier(x.SourceId), ct);
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

        var newImages = notExistUrls
            .Select(notExistUrl => new Image { Url = notExistUrl })
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