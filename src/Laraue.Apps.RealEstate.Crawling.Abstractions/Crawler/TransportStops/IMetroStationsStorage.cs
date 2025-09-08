namespace Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.TransportStops;

public interface IMetroStationsStorage
{
    /// <summary>
    /// Return all metro station identifiers.
    /// </summary>
    /// <returns></returns>
    MetroStationData[] GetMetroStations();
}