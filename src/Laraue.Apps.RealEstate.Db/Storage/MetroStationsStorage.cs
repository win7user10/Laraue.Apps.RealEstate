using Laraue.Apps.RealEstate.Crawling.Contracts.Crawler.TransportStops;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Laraue.Apps.RealEstate.Db.Storage;

public sealed class MetroStationsStorage : IMetroStationsStorage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _memoryCache;

    public MetroStationsStorage(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
    }

    public MetroStationData[] GetMetroStations()
    {
        return _memoryCache.GetOrCreate(
            $"Stations",
            entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.MaxValue;
                
                using var scope = _serviceProvider.CreateScope();
                
                return scope.ServiceProvider
                    .GetRequiredService<AdvertisementsDbContext>()
                    .TransportStops
                    .Select(x => new MetroStationData(
                        x.Id,
                        x.Name,
                        x.Priority,
                        x.Color))
                    .ToArray();
            })!;
    }
}