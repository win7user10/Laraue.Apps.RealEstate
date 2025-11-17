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
    
    [Range(0, 100_000_000_000)]
    public decimal? MinPrice { get; init; }
    
    [Range(0, 100_000_000_000)]
    public decimal? MaxPrice { get; init; }

    [Range(0, 10)]
    public int? MinRenovationRating { get; init; }
    
    [Range(0, 10)]
    public int? MaxRenovationRating { get; init; }
    
    [Range(0, 10_000_000)]
    public decimal? MinPerSquareMeterPrice { get; init; }
    
    [Range(0, 10_000_000)]
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
    
    public string? SearchString { get; init; } 
}