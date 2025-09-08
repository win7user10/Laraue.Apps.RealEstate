using Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts;
using Laraue.Crawling.Abstractions;
using PuppeteerSharp;

namespace Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;

public interface ICrawlingSchema : ICompiledDocumentSchema<IElementHandle, HtmlSelector, CrawlingResult>
{
}