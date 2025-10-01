using System.ComponentModel.DataAnnotations;
using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Core.DataAccess.Contracts;

namespace Laraue.Apps.RealEstate.ApiHost.Requests;

public sealed record GetAdvertisementsRequest : IPaginatedRequest
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

    [Range(0, 1)]
    public double? MinRenovationRating { get; init; }
    
    [Range(0, 1)]
    public double? MaxRenovationRating { get; init; }
    
    public decimal? MinPerSquareMeterPrice { get; init; }
    
    public decimal? MaxPerSquareMeterPrice { get; init; }
    
    [Range(0, 30000)]
    public decimal? MinSquare { get; init; }

    public bool ExcludeFirstFloor { get; init; }
    
    public bool ExcludeLastFloor { get; init; }
    
    public byte? MinMetroStationPriority { get; init; }

    public AdvertisementsSort SortBy { get; init; }
    
    public SortOrder SortOrder { get; init; }

    public IList<long>? MetroIds { get; init; }
    
    public IList<int>? RoomsCount { get; init; }
    
    public AdvertisementSource? Source { get; init; }
}