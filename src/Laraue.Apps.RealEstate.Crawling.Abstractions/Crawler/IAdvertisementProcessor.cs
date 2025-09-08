using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts;

namespace Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;

public interface IAdvertisementProcessor
{
    /// <summary>
    /// Advertisement source of this processor.
    /// </summary>
    AdvertisementSource Source { get; }
    
    /// <summary>
    /// Process advertisements.
    /// </summary>
    /// <param name="advertisements"></param>
    /// <param name="ct"></param>
    /// <returns>Identifiers of the passed advs that have been saved or updated.</returns>
    Task<HashSet<long>> ProcessAsync(
        Advertisement[] advertisements,
        CancellationToken ct = default);
}