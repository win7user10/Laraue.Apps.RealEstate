namespace Laraue.Apps.RealEstate.Crawling.Impl;

public interface ICrawlerLocker
{
    Task LockAsync(string key, CancellationToken cancellationToken);
}

public class CrawlerLocker
{
    
}