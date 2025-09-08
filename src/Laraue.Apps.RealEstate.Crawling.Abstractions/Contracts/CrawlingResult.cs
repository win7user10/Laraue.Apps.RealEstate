using Laraue.Crawling.Abstractions;

namespace Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts;

public sealed class CrawlingResult : ICrawlingModel
{
    public Advertisement[] Advertisements { get; init; } = [];
}