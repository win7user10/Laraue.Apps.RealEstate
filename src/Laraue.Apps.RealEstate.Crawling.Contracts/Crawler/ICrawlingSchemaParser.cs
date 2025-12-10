using Laraue.Apps.RealEstate.Crawling.Contracts.Contracts;

namespace Laraue.Apps.RealEstate.Crawling.Contracts.Crawler;

public interface ICrawlingSchemaParser
{
    public Task<CrawlingResult> ParseLinkAsync(string link, CancellationToken cancellationToken = default);
}