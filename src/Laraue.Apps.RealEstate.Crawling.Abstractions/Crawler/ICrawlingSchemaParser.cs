using Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts;

namespace Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;

public interface ICrawlingSchemaParser
{
    public Task<CrawlingResult> ParseLinkAsync(string link, CancellationToken cancellationToken = default);
}