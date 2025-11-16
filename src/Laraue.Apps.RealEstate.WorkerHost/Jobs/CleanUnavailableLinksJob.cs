using Laraue.Apps.RealEstate.Db;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.Extensions.Hosting;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Laraue.Apps.RealEstate.WorkerHost.Jobs;

public class CleanUnavailableLinksJob(
    AdvertisementsDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    HttpClient client,
    ILogger<CleanUnavailableLinksJob> logger)
    : BaseJob
{
    private readonly TimeSpan _timeToRecheckAdvertisement = TimeSpan.FromDays(7);
    private readonly TimeSpan _timeBetweenJobsRun = TimeSpan.FromMinutes(5);
    private const int BathSize = 50;

    public override async Task<TimeSpan> ExecuteAsync(
        JobState<EmptyJobData> jobState,
        CancellationToken stoppingToken = default)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var imagesToCheck = await dbContext.Images
                .Where(i => i.LastAvailableAt < dateTimeProvider.UtcNow - _timeToRecheckAdvertisement)
                .Take(BathSize)
                .Select(x => new
                {
                    x.Id,
                    x.Url
                })
                .ToArrayAsyncEF(stoppingToken);

            if (imagesToCheck.Length == 0)
            {
                return _timeBetweenJobsRun;
            }

            var failed = new HashSet<long>();
            
            foreach (var image in imagesToCheck)
            {
                try
                {
                    var response = await client.GetAsync(image.Url, stoppingToken);
                    logger.LogInformation("Planing to remove image '{Url}' from DB", image.Url);
                    if (!response.IsSuccessStatusCode)
                    {
                        failed.Add(image.Id);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Job takes the error, continue after restart");
                    return TimeSpan.FromMinutes(5);
                }
            }

            var removedCount = await dbContext.Images
                .Where(i => failed.Contains(i.Id))
                .ExecuteDeleteAsync(stoppingToken);

            var toUpdate = imagesToCheck
                .Select(x => x.Id)
                .Except(failed);
            
            var updatedCount = await dbContext.Images
                .Where(i => toUpdate.Contains(i.Id))
                .ExecuteUpdateAsync(update => update
                    .SetProperty(x => x.LastAvailableAt, dateTimeProvider.UtcNow),
                    stoppingToken);
            
            logger.LogInformation(
                "Clean images cycle finished, updated ({UpdatedCount}), deleted: ({DeletedCount})",
                updatedCount,
                removedCount);
        }

        return _timeBetweenJobsRun;
    }
}