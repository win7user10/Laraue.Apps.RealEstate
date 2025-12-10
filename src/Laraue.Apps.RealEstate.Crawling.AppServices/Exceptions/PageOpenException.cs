namespace Laraue.Apps.RealEstate.Crawling.AppServices.Exceptions;

public class PageOpenException : Exception
{
    public PageOpenException(string message, Exception e) : base(message, e)
    {}
    
    public PageOpenException(string message) : base(message)
    {}
}