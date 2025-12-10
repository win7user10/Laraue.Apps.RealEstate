using Laraue.Apps.RealEstate.Crawling.Contracts;

namespace Laraue.Apps.RealEstate.Crawling.AppServices;

public interface ICrawlingSchemaParser
{
    public Task<CrawlingResult> ParseLinkAsync(string link, CancellationToken cancellationToken = default);
}