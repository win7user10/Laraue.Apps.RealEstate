using Laraue.Crawling.Abstractions;

namespace Laraue.Apps.RealEstate.Crawling.Contracts.Contracts;

public sealed class CrawlingResult : ICrawlingModel
{
    public Advertisement[] Advertisements { get; init; } = [];
}