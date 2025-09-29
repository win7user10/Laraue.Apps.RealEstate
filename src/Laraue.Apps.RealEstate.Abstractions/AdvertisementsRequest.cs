using Laraue.Core.DataAccess.Contracts;

namespace Laraue.Apps.RealEstate.Abstractions;

public sealed record AdvertisementsRequest : IPaginatedRequest
{
    public Filter Filter { get; init; } = new();
    public PaginationData Pagination { get; init; } = new();
}

public class Filter
{
    public DateTime? MinDate { get; init; }
    
    public DateTime? MaxDate { get; init; }
    
    public decimal? MinPrice { get; init; }
    
    public decimal? MaxPrice { get; init; }

    public double? MinRenovationRating { get; init; }
    
    public double? MaxRenovationRating { get; init; }
    
    public decimal? MinPerSquareMeterPrice { get; init; }
    
    public decimal? MaxPerSquareMeterPrice { get; init; }

    public decimal? MinSquare { get; init; }

    public bool ExcludeFirstFloor { get; init; }
    
    public bool ExcludeLastFloor { get; init; }

    public byte? MinMetroStationPriority { get; init; }
    
    public AdvertisementsSort SortBy { get; init; }
    
    public SortOrder SortOrderBy { get; init; }
    
    public IList<long>? MetroIds { get; init; }
    
    public IList<int>? RoomsCount { get; init; }

    public int? DistanceInMinutes { get; set; }
}