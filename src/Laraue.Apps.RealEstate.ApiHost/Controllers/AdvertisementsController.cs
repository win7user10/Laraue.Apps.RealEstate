using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.ApiHost.Requests;
using Laraue.Core.DataAccess.Contracts;
using Microsoft.AspNetCore.Mvc;
using Filter = Laraue.Apps.RealEstate.Abstractions.Filter;

namespace Laraue.Apps.RealEstate.ApiHost.Controllers;

[Route("/api/advertisements")]
[ApiController]
public sealed class AdvertisementsController : ControllerBase
{
   private readonly IAdvertisementStorage _storage;

   public AdvertisementsController(IAdvertisementStorage storage)
   {
      _storage = storage;
   }

   [HttpPost("list")]
   public Task<IShortPaginatedResult<AdvertisementDto>> GetAdvertisementsAsync([FromBody] GetAdvertisementsRequest request)
   {
      return _storage.GetAdvertisementsAsync(
         new AdvertisementsRequest
         {
            Filter = new Filter()
            {
               MaxDate = request.Filter.MaxDate,
               MinDate = request.Filter.MinDate,
               MetroIds = request.Filter.MetroIds,
               MaxPrice = request.Filter.MaxPrice,
               MinPrice = request.Filter.MinPrice,
               ExcludeFirstFloor = request.Filter.ExcludeFirstFloor,
               ExcludeLastFloor = request.Filter.ExcludeLastFloor,
               MaxRenovationRating = request.Filter.MaxRenovationRating,
               MinRenovationRating = request.Filter.MinRenovationRating,
               MaxPerSquareMeterPrice = request.Filter.MaxPerSquareMeterPrice,
               MinPerSquareMeterPrice = request.Filter.MinPerSquareMeterPrice,
               MinMetroStationPriority = request.Filter.MinMetroStationPriority,
               SortOrderBy = request.Filter.SortOrder,
               SortBy = request.Filter.SortBy,
               MinSquare = request.Filter.MinSquare,
               RoomsCount = request.Filter.RoomsCount,
            },
            Pagination = new PaginationData
            {
               Page = request.Pagination.Page,
               PerPage = request.Pagination.PerPage,
            }
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