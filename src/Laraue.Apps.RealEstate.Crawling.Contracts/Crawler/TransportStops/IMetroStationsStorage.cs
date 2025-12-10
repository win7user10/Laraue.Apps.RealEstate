namespace Laraue.Apps.RealEstate.Crawling.Contracts.Crawler.TransportStops;

public interface IMetroStationsStorage
{
    /// <summary>
    /// Return all metro station identifiers.
    /// </summary>
    /// <returns></returns>
    MetroStationData[] GetMetroStations();
}