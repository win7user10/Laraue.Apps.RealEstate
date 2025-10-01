﻿using System.Diagnostics.CodeAnalysis;
using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.TransportStops;
using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.Db.Models;
using Laraue.Core.DataAccess.EFCore.Extensions;
using Laraue.Core.DateTime.Services.Abstractions;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Advertisement = Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts.Advertisement;

namespace Laraue.Apps.RealEstate.Crawling.Impl;

public abstract class BaseAdvertisementProcessor<TExternalIdentifier> : IAdvertisementProcessor
    where TExternalIdentifier : struct
{
    public AdvertisementSource Source { get; }
    private readonly AdvertisementsDbContext _dbContext;
    private readonly IMetroStationsStorage _metroStationsStorage;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<BaseAdvertisementProcessor<TExternalIdentifier>> _logger;

    private IDictionary<string, MetroStationData>? _externalPublicStopsIds;
    private IDictionary<long, MetroStationData>? _systemPublicStopsIds;

    protected BaseAdvertisementProcessor(
        AdvertisementSource source,
        AdvertisementsDbContext dbContext,
        IMetroStationsStorage metroStationsStorage,
        IDateTimeProvider dateTimeProvider,
        ILogger<BaseAdvertisementProcessor<TExternalIdentifier>> logger)
    {
        Source = source;
        _dbContext = dbContext;
        _metroStationsStorage = metroStationsStorage;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }
    
    public async Task<HashSet<long>> ProcessAsync(
        Advertisement[] advertisements,
        CancellationToken ct = default)
    {
        await using var transaction = await _dbContext.BeginTransactionIfNotStartedAsync();
        
        var updatedAdvertisements = await UpdateAdvertisementsAsync(advertisements, ct);

        await UpdateImageLinksAsync(updatedAdvertisements, ct);
        
        await UpdateTransportStopsAsync(updatedAdvertisements, ct);
        
        await transaction.CommitAsync(ct);

        return updatedAdvertisements.Keys.ToHashSet();
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
                        AdvertisementId = advertisement.Key
                    });
                }
            }
        }

        await _dbContext.SaveChangesAsync(ct);
    }
    
    private async Task<Dictionary<long, Advertisement>> UpdateAdvertisementsAsync(
        Advertisement[] advertisements,
        CancellationToken ct = default)
    {
        var existsAdvertisements = await GetExistsAdvertisementsAsync(advertisements, ct);
        
        var updatedAdvertisements = new List<Db.Models.Advertisement>();
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

            var dbAdvertisement = new Db.Models.Advertisement();
            
            if (!existsAdvertisements.TryGetValue(parsedPage.Id, out var advertisement))
            {
                _dbContext.Add(dbAdvertisement);
                
                FillAdvertisement(dbAdvertisement, parsedPage);
            }
            else
            {
                if (advertisement.UpdatedAt >= parsedPage.UpdatedAt)
                {
                    continue;
                }
                
                dbAdvertisement.Id = advertisement.Id;
                _dbContext.Attach(dbAdvertisement);
                FillAdvertisement(dbAdvertisement, parsedPage);
            }

            updatedAdvertisements.Add(dbAdvertisement);
        }
        
        var updatedCount = await _dbContext.SaveChangesAsync(ct);
        
        _logger.LogInformation("Updated {Count} entities in DB", updatedCount);

        return updatedAdvertisements
            .Join(
                advertisements,
                x => x.SourceId,
                x => x.Id,
                (model, parsed) => (model, parsed))
            .ToDictionary(x => x.model.Id, x => x.parsed);
    }

    private void FillAdvertisement(
        Db.Models.Advertisement model,
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