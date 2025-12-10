using Laraue.Apps.RealEstate.Crawling.Contracts.Contracts;
using Laraue.Apps.RealEstate.Crawling.Contracts.Crawler;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Crawling.Crawler;

namespace Laraue.Apps.RealEstate.Crawling.Impl;

public interface ISessionInterrupter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="processResult"></param>
    /// <param name="sessionUpdatedAdvertisementIds">Items already crawled in the current session.</param>
    /// <param name="options"></param>
    void ThrowIfRequired(
        ProcessResult processResult,
        HashSet<long> sessionUpdatedAdvertisementIds,
        BaseCrawlerServiceOptions options);
}

public class SessionInterrupter(
    IDateTimeProvider dateTimeProvider) : ISessionInterrupter
{
    public void ThrowIfRequired(
        ProcessResult processResult,
        HashSet<long> sessionUpdatedAdvertisementIds,
        BaseCrawlerServiceOptions options)
    {
        // If some correct advs were not updated need to check the reason.
        if (processResult.OutdatedItemsIds.Count > 0)
        {
            // Need to check adv identifiers. If all adv ids from this session, it just means
            // that crawler was powered off for some time when new advs appeared.
            // Else it means the crawler found items that were stored in previous sessions. 
            if (!processResult.OutdatedItemsIds.All(sessionUpdatedAdvertisementIds.Contains))
            {
                throw new SessionInterruptedException("Already parsed advertisements found");
            }
        }
        
        // If the system found advs that is too old (based on options), the session should be finished.
        // It prevents the whole resource parsing at the first run.
        if (!AreAllAdvertisementsActual(processResult.UpdatedAdvertisements.Values, options))
        {
            throw new SessionInterruptedException("Too old advertisements found");
        }
    }
    
    private bool AreAllAdvertisementsActual(
        IEnumerable<Advertisement> advertisements,
        BaseCrawlerServiceOptions options)
    {
        var latestAdvertisementDate = advertisements.Max(x => x.UpdatedAt); // 2 days before | 1 day before -> 1 day before
        
        // now - 10 min before = - 10 min
        // -10 min <= 15 day -> true
        return dateTimeProvider.UtcNow - latestAdvertisementDate
               <= options.MaxAdvertisementDateOffset;
    }
}