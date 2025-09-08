using Laraue.Core.DataAccess.Contracts;

namespace Laraue.Apps.RealEstate.Abstractions;

public interface IAdvertisementStorage
{
    /// <summary>
    /// Get top advertisements for the passed criteria.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<IShortPaginatedResult<AdvertisementDto>> GetAdvertisementsAsync(
        AdvertisementsRequest request);

    /// <summary>
    /// Get common chart with data for the each day in the passed date range.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<IList<MainChartDayItemDto>> GetMainChartAsync(
        RangeChartRequest request);
    
    /// <summary>
    /// Get common chart with data for the each day in specified dates.
    /// </summary>
    /// <param name="dates"></param>
    /// <returns></returns>
    Task<IList<MainChartDayItemDto>> GetMainChartAsync(
        IEnumerable<DateTime> dates);
}