using Laraue.Apps.RealEstate.Crawling.Contracts.Contracts;
using Laraue.Crawling.Abstractions;
using PuppeteerSharp;

namespace Laraue.Apps.RealEstate.Crawling.Contracts.Crawler;

public interface ICrawlingSchema : ICompiledDocumentSchema<IElementHandle, HtmlSelector, CrawlingResult>
{
}