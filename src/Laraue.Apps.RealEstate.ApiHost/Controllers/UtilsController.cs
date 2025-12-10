using Laraue.Apps.RealEstate.Crawling.AppServices;
using Laraue.Apps.RealEstate.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Laraue.Apps.RealEstate.ApiHost.Controllers;

[Route("/api/utils")]
public sealed class UtilsController : ControllerBase
{
    private readonly AdvertisementsDbContext _dbContext;
    private readonly ILogger<UtilsController> _logger;

    public UtilsController(
        AdvertisementsDbContext dbContext,
        ILogger<UtilsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    [HttpPost]
    [Authorize]
    [Route("recalculate-houses-normalization")]
    public async Task RecalculateNormalizedAddresses(CancellationToken cancellationToken)
    {
        var lastId = 0L;
        const int batchSize = 200;
        
        while (true)
        {
            var houses = await _dbContext.Houses
                .Where(x => x.Id > lastId)
                .OrderBy(x => x.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (houses.Count == 0)
            {
                break;
            }

            foreach (var house in houses)
            {
                house.NumberNormalized = AddressNormalizer.NormalizeForSearch(house.Number);
            }
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear();
            
            lastId = houses.Last().Id;
        }
        
        _logger.LogInformation("Sync finished, last Id: {Id}", lastId);
    }
}