using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;
using Laraue.Apps.RealEstate.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Laraue.Apps.RealEstate.WorkerHost.Controllers;

[Route("/api/utils")]
public sealed class UtilsController : ControllerBase
{
    private readonly AdvertisementsDbContext _dbContext;
    private readonly ILogger<UtilsController> _logger;
    private readonly IAdvertisementComputedFieldsCalculator _computedFieldsCalculator;

    public UtilsController(
        AdvertisementsDbContext dbContext,
        ILogger<UtilsController> logger,
        IAdvertisementComputedFieldsCalculator computedFieldsCalculator)
    {
        _dbContext = dbContext;
        _logger = logger;
        _computedFieldsCalculator = computedFieldsCalculator;
    }

    [HttpPost]
    [Route("recalculate-predicted-prices")]
    public async Task RecalculateAveragePricesAsync(CancellationToken cancellationToken)
    {
        var lastId = 0L;
        const int batchSize = 200;
        
        while (true)
        {
            var advertisements = await _dbContext.Advertisements
                .Include(x => x.TransportStops)
                .ThenInclude(x => x.TransportStop)
                .Where(x => x.Id > lastId)
                .OrderBy(x => x.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (advertisements.Count == 0)
            {
                break;
            }

            foreach (var advertisement in advertisements)
            {
                var computedFields = _computedFieldsCalculator.ComputeFields(
                    new AdvertisementData(
                        advertisement.Square,
                        advertisement.TotalPrice,
                        advertisement.RenovationRating,
                        advertisement.FloorNumber,
                        advertisement.TotalFloorsNumber,
                        advertisement.TransportStops
                            .Select(ts => new TransportStopData(
                                ts.TransportStop!.Priority,
                                ts.DistanceInMinutes,
                                ts.DistanceType))
                            .ToArray()));
                
                advertisement.UpdateComputedFields(computedFields);
            }
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear();
            
            lastId = advertisements.Last().Id;
        }
        
        _logger.LogInformation("Sync finished, last Id: {Id}", lastId);
    }
}