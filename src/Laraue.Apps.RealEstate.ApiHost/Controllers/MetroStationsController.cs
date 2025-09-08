using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.TransportStops;
using Microsoft.AspNetCore.Mvc;

namespace Laraue.Apps.RealEstate.ApiHost.Controllers;

[Route("api/metro-stations")]
public class MetroStationsController : ControllerBase
{
    private readonly IMetroStationsStorage _metroStationsStorage;

    public MetroStationsController(IMetroStationsStorage metroStationsStorage)
    {
        _metroStationsStorage = metroStationsStorage;
    }

    [HttpGet]
    public MetroStationData[] GetMetroStations()
    {
        return _metroStationsStorage.GetMetroStations();
    }
}