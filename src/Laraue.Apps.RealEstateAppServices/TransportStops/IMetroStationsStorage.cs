namespace Laraue.Apps.RealEstateAppServices.TransportStops;

public interface IMetroStationsStorage
{
    /// <summary>
    /// Return all metro station identifiers.
    /// </summary>
    /// <returns></returns>
    MetroStationData[] GetMetroStations();
}