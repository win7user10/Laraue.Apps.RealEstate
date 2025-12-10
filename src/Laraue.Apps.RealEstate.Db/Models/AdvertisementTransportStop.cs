using Laraue.Apps.RealEstate.Contracts;

namespace Laraue.Apps.RealEstate.Db.Models;

public sealed class AdvertisementTransportStop
{
    public long Id { get; init; }

    public Advertisement? Advertisement { get; init; }

    public TransportStop? TransportStop { get; init; }
    
    public long AdvertisementId { get; init; }
    
    public long TransportStopId { get; init; }
    
    public int DistanceInMinutes { get; init; }
    
    public DistanceType DistanceType { get; init; }
}