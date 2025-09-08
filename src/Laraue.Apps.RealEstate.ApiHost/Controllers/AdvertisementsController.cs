using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.ApiHost.Requests;
using Laraue.Core.DataAccess.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Laraue.Apps.RealEstate.ApiHost.Controllers;

[Route("/api/advertisements")]
public sealed class AdvertisementsController : ControllerBase
{
   private readonly IAdvertisementStorage _storage;

   public AdvertisementsController(IAdvertisementStorage storage)
   {
      _storage = storage;
   }

   [HttpGet("list")]
   public Task<IShortPaginatedResult<AdvertisementDto>> GetAdvertisementsAsync(GetAdvertisementsRequest request)
   {
      return _storage.GetAdvertisementsAsync(
         new AdvertisementsRequest
         {
            Page = request.Page,
            MaxDate = request.MaxDate,
            MinDate = request.MinDate,
            MetroIds = request.MetroIds,
            PerPage = request.PerPage,
            MaxPrice = request.MaxPrice,
            MinPrice = request.MinPrice,
            ExcludeFirstFloor = request.ExcludeFirstFloor,
            ExcludeLastFloor = request.ExcludeLastFloor,
            MaxRenovationRating = request.MaxRenovationRating,
            MinRenovationRating = request.MinRenovationRating,
            MaxPerSquareMeterPrice = request.MaxPerSquareMeterPrice,
            MinPerSquareMeterPrice = request.MinPerSquareMeterPrice,
            MinMetroStationPriority = request.MinMetroStationPriority,
            SortOrderBy = request.SortOrder,
            SortBy = request.SortBy,
            MinSquare = request.MinSquare,
            RoomsCount = request.RoomsCount,
         });
   }

   [HttpGet("chart")]
   public Task<IList<MainChartDayItemDto>> GetMainChartAsync(GetMainChartRequest request)
   {
      return _storage.GetMainChartAsync(
         new RangeChartRequest
         {
            MaxDate = request.MaxDate,
            MinDate = request.MinDate
         });
   }
}