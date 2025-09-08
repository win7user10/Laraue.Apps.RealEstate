using System.ComponentModel.DataAnnotations.Schema;
using Laraue.Apps.RealEstate.Abstractions;

namespace Laraue.Apps.RealEstate.Db.Models;

public sealed record Selection
{
    public long Id { get; set; }

    public required string Name { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }

    public MayBeRelativeDate? MinDate { get; set; }
    
    public MayBeRelativeDate? MaxDate { get; set; }
    
    public decimal? MinPrice { get; set; }
    
    public decimal? MaxPrice { get; set; }

    public double? MinRenovationRating { get; set; }
    
    public double? MaxRenovationRating { get; set; }
    
    public decimal? MinPerSquareMeterPrice { get; set; }
    
    public decimal? MaxPerSquareMeterPrice { get; set; }

    public decimal? MinSquare { get; set; }

    public bool ExcludeFirstFloor { get; set; }
    
    public bool ExcludeLastFloor { get; set; }

    public byte? MinMetroStationPriority { get; set; }
    
    public AdvertisementsSort SortBy { get; set; }
    
    public SortOrder SortOrderBy { get; set; }
    
    [Column(TypeName = "jsonb")]
    public IList<long>? MetroIds { get; set; }
    
    [Column(TypeName = "jsonb")]
    public IList<int>? RoomsCount { get; set; }

    /// <summary>
    /// Interval between sending selection result to the user.
    /// </summary>
    public TimeSpan? NotificationInterval { get; set; }

    /// <summary>
    /// The last date when selection has been sent via the job.
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// How much advertisement should be sent.
    /// </summary>
    public int PerPage { get; set; }
}