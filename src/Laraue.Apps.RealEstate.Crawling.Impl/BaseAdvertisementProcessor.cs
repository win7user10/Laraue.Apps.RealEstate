using System.Diagnostics.CodeAnalysis;
using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.TransportStops;
using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.Db.Models;
using Laraue.Apps.RealEstate.Prediction.Abstractions;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Advertisement = Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts.Advertisement;
using TransportStop = Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts.TransportStop;

namespace Laraue.Apps.RealEstate.Crawling.Impl;

public abstract class BaseAdvertisementProcessor<TExternalIdentifier> : IAdvertisementProcessor
    where TExternalIdentifier : struct
{
    public AdvertisementSource Source { get; }
    private readonly AdvertisementsDbContext _dbContext;
    private readonly IRemoteImagesPredictor _imagesPredictor;
    private readonly IAverageRatingCalculator _calculator;
    private readonly IAdvertisementComputedFieldsCalculator _computedFieldsCalculator;
    private readonly IMetroStationsStorage _metroStationsStorage;

    private IDictionary<string, MetroStationData>? _externalPublicStopsIds;
    private IDictionary<long, MetroStationData>? _systemPublicStopsIds;

    protected BaseAdvertisementProcessor(
        AdvertisementSource source,
        AdvertisementsDbContext dbContext,
        IRemoteImagesPredictor imagesPredictor,
        IAverageRatingCalculator calculator,
        IAdvertisementComputedFieldsCalculator computedFieldsCalculator,
        IMetroStationsStorage metroStationsStorage)
    {
        Source = source;
        _dbContext = dbContext;
        _imagesPredictor = imagesPredictor;
        _calculator = calculator;
        _computedFieldsCalculator = computedFieldsCalculator;
        _metroStationsStorage = metroStationsStorage;
    }
    
    public async Task<HashSet<long>> ProcessAsync(
        Advertisement[] advertisements,
        CancellationToken ct = default)
    {
        var existsAdvertisements = await GetExistsAdvertisementsAsync(advertisements, ct);
        
        var urlsToPredict = await GetNotPredictedImageUrlsAsync(advertisements);
        var imagePredictions = await _imagesPredictor.PredictAsync(urlsToPredict, ct);
        
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
                
                UpdateAdvertisement(advertisement, parsedPage, imagePredictions);
            }
            else
            {
                if (advertisement.UpdatedAt >= parsedPage.UpdatedAt)
                {
                    continue;
                }
                
                UpdateAdvertisement(advertisement, parsedPage, imagePredictions);
            }

            var computedFields = _computedFieldsCalculator.ComputeFields(
                new AdvertisementData(
                    advertisement.Square,
                    advertisement.TotalPrice,
                    advertisement.RenovationRating,
                    advertisement.FloorNumber,
                    advertisement.TotalFloorsNumber,
                    advertisement.TransportStops
                        .Select(ts => new TransportStopData(
                            GetPublicStopCached(ts.TransportStopId).Priority,
                            ts.DistanceInMinutes,
                            ts.DistanceType))
                        .ToArray()));
            
            advertisement.UpdateComputedFields(computedFields);
            updatedAdvertisements.Add(advertisement);
        }
        
        await _dbContext.SaveChangesAsync(ct);
        
        return updatedAdvertisements.Select(x => x.Id).ToHashSet();
    }

    private void UpdateAdvertisement(
        Db.Models.Advertisement advertisement,
        Advertisement page,
        IDictionary<string, PredictionResult> imagePredictions)
    {
        // Upd common fields
        advertisement.Link = page.Link;
        advertisement.SourceType = Source;
        advertisement.Square = page.Square;
        advertisement.SourceId = page.Id;
        advertisement.FloorNumber = page.FloorNumber;
        advertisement.RoomsCount = page.RoomsCount;
        advertisement.TotalFloorsNumber = page.TotalFloorsNumber;
        advertisement.ShortDescription = page.ShortDescription;
        advertisement.TotalPrice = page.TotalPrice;
        advertisement.UpdatedAt = page.UpdatedAt.GetValueOrDefault();
        advertisement.CrawledAt = DateTime.UtcNow;
        advertisement.FlatType = page.FlatType;
        
        // upd images via sync to actual state logic
        foreach (var removedImage in advertisement.Images
            .Where(x => !page.ImageLinks.Contains(x.Url)))
        {
            _dbContext.Remove(removedImage);
        }

        var existsLinks = advertisement.Images.Select(y => y.Url);
        var newLinks = page.ImageLinks.Except(existsLinks);
        var newPredictions = GetPredictedImagesModels(newLinks, imagePredictions);

        foreach (var newPrediction in newPredictions)
        {
            advertisement.Images.Add(newPrediction);
        }
        
        // Update average rating
        advertisement.RenovationRating = _calculator.Calculate(advertisement.Images
            .Select(x => new PredictionResult
            {
                RenovationRating = x.RenovationRating,
            }));
        
        // upd transport stops via add new logic
        var existStops = advertisement.TransportStops
            .Select(x => x.TransportStopId)
            .ToHashSet();
        
        foreach (var transportStop in page.TransportStops ?? Array.Empty<TransportStop>())
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
                
            advertisement.TransportStops.Add(new AdvertisementTransportStop
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
            .Include(x => x.Images)
            .Include(x => x.TransportStops)
            .ThenInclude(x => x.TransportStop)
            .Where(x => x.SourceType == Source && 
                advertisements
                    .Select(y => y.Id.ToString())
                    .Contains(x.SourceId))
            .ToDictionaryAsyncEF(x => ParseIdentifier(x.SourceId), ct);
    }
    
    private async Task<IEnumerable<string>> GetNotPredictedImageUrlsAsync(
        IEnumerable<Advertisement> advertisements)
    {
        var allAdvertisementImagesUrls = advertisements
            .SelectMany(y => y.ImageLinks)
            .ToArray();
        
        var existUrls = await _dbContext.AdvertisementImages
            .Where(x => allAdvertisementImagesUrls.Contains(x.Url))
            .Select(x => x.Url)
            .ToListAsync();
        
        return allAdvertisementImagesUrls.Except(existUrls);
    }
    
    private static IEnumerable<AdvertisementImage> GetPredictedImagesModels(
        IEnumerable<string> links,
        IDictionary<string, PredictionResult> predictionDictionary)
    {
        foreach (var imageLink in links)
        {
            if (predictionDictionary.TryGetValue(imageLink, out var prediction))
            {
                yield return new AdvertisementImage
                {
                    Url = imageLink,
                    RenovationRating = prediction.RenovationRating,
                    Tags = prediction.Tags,
                    Decription = prediction.Description
                };
            }
        }
    }
}