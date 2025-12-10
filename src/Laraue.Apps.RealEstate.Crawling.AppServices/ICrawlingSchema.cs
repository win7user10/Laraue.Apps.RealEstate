using Laraue.Apps.RealEstate.Crawling.Contracts;
using Laraue.Crawling.Abstractions;
using PuppeteerSharp;

namespace Laraue.Apps.RealEstate.Crawling.AppServices;

public interface ICrawlingSchema : ICompiledDocumentSchema<IElementHandle, HtmlSelector, CrawlingResult>
{
}