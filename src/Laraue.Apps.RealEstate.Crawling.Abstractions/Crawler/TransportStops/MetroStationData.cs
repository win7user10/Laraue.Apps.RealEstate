namespace Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler.TransportStops;

/// <summary>
/// Metro stations data from the <see cref="IMetroStationsStorage"/>.
/// </summary>
public sealed record MetroStationData(
    long Id,
    string Name,
    int Priority,
    string Color);