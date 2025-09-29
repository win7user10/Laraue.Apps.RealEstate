using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Db.Models;
using Laraue.Core.DataAccess.Contracts;

namespace Laraue.Apps.RealEstate.Db.Extensions;

public static class SelectionExtensions
{
    public static AdvertisementsRequest ToAdvertisementsRequest(
        this Selection selection,
        bool getMinDateFromNotificationInterval,
        int page = 0)
    {
        return selection.ToAdvertisementsRequest(
            minDate: getMinDateFromNotificationInterval && selection.NotificationInterval is not null
                ? DateTime.UtcNow.Add(-selection.NotificationInterval.Value)
                : selection.MinDate?.Value ?? DateTime.MinValue,
            maxDate: selection.MaxDate?.Value ?? DateTime.UtcNow,
            page: page);
    }

    public static AdvertisementsRequest ToAdvertisementsRequest(
        this Selection selection,
        DateTime minDate,
        DateTime maxDate,
        int page = 0)
    {
        return new AdvertisementsRequest
        {
            Filter = new Filter
            {
                MinRenovationRating = selection.MinRenovationRating,
                MaxPrice = selection.MaxPrice,
                MinPrice = selection.MinPrice,
                MinSquare = selection.MinSquare,
                RoomsCount = selection.RoomsCount,
                MetroIds = selection.MetroIds,
                ExcludeFirstFloor = selection.ExcludeFirstFloor,
                ExcludeLastFloor = selection.ExcludeLastFloor,
                MaxRenovationRating = selection.MaxRenovationRating,
                SortOrderBy = selection.SortOrderBy,
                MaxDate = maxDate,
                MaxPerSquareMeterPrice = selection.MaxPerSquareMeterPrice,
                MinPerSquareMeterPrice = selection.MinPerSquareMeterPrice,
                SortBy = selection.SortBy,
                MinMetroStationPriority = selection.MinMetroStationPriority,
                MinDate = minDate,
            },
            Pagination = new PaginationData
            {
                PerPage = selection.PerPage,
                Page = page,
            }
        };
    }
}