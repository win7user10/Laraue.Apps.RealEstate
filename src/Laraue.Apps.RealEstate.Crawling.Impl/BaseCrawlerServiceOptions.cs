namespace Laraue.Apps.RealEstate.Crawling.Impl;

public abstract class BaseCrawlerServiceOptions
{
    /// <summary>
    /// How much time should be passed after crawler should start
    /// a new crawler session after last has been finished.
    /// </summary>
    public TimeSpan TimeBetweenSessions { get; init; }
    
    /// <summary>
    /// Maximum range from the current date time to take advertisements
    /// in a crawler session.
    /// </summary>
    public TimeSpan MaxAdvertisementDateOffset { get; init; }

    /// <summary>
    /// Low range of the sleeping timeout while crawling. 
    /// </summary>
    public int MinTimeoutBeforeSwitchToNextPage { get; init; }

    /// <summary>
    /// High range of the sleeping timeout while crawling. 
    /// </summary>
    public int MaxTimeoutBeforeSwitchToNextPage { get; init; }
}