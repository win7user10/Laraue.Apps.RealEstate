namespace Laraue.Apps.RealEstate.AppServices.TransportStops;

public interface IMetroStationsStorage
{
    /// <summary>
    /// Return all metro station identifiers.
    /// </summary>
    /// <returns></returns>
    MetroStationData[] GetMetroStations();
}