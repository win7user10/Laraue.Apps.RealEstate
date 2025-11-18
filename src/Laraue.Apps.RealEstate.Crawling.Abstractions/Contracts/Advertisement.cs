using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;
using Laraue.Crawling.Abstractions;

namespace Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts;

/// <summary>
/// The model that was parsed.
/// </summary>
public sealed class Advertisement : ICrawlingModel
{
    /// <summary>
    /// Advertisement identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Link to the advertisement.
    /// </summary>
    public string Link { get; init; } = string.Empty;

    /// <summary>
    /// Advertisement total price.
    /// </summary>
    public long TotalPrice { get; init; }

    /// <summary>
    /// Rooms count or null if was not found.
    /// </summary>
    public int RoomsCount { get; init; }

    /// <summary>
    /// The apartments square, m2.
    /// </summary>
    public decimal Square { get; init; }

    /// <summary>
    /// The floor number.
    /// </summary>
    public int FloorNumber { get; init; }
    
    /// <summary>
    /// Total floors number in the building.
    /// </summary>
    public int TotalFloorsNumber { get; init; }
    
    /// <summary>
    /// Advertisement description.
    /// </summary>
    public string? ShortDescription { get; init; }

    /// <summary>
    /// When the advertisement was updated last time.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Image links related to this advertisement.
    /// </summary>
    public string[] ImageLinks { get; init; } = [];

    /// <summary>
    /// The flat type.
    /// </summary>
    public FlatType FlatType { get; init; }

    public TransportStop[]? TransportStops { get; init; }

    /// <summary>
    /// The flat address.
    /// </summary>
    public FlatAddress? FlatAddress { get; init; }
}