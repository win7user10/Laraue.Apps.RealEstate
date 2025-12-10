using System.Diagnostics;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.Db;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.Extensions.Hosting;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Laraue.Apps.RealEstate.WorkerHost.Jobs;

public class ArchiveAdvertisementsWithoutImagesJob(
    AdvertisementsDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    ILogger<ArchiveAdvertisementsWithoutImagesJob> logger)
    : BaseJob
{
    private readonly TimeSpan _timeBetweenJobsRun = TimeSpan.FromMinutes(5);
    private const int BatchSize = 150;
    
    public override async Task<TimeSpan> ExecuteAsync(
        JobState<EmptyJobData> jobState,
        CancellationToken stoppingToken = default)
    {
        var sw = new Stopwatch();

        while (!stoppingToken.IsCancellationRequested)
        {
            sw.Restart();
            
            var advertisementsToArchive = await dbContext.Advertisements
                .Where(a => a.LinkedImages.Count == 0)
                .Where(a => a.DeletedAt == null)
                .OrderBy(a => a.Id)
                .Take(BatchSize)
                .Select(x => x.Id)
                .ToArrayAsyncEF(stoppingToken);

            if (advertisementsToArchive.Length == 0)
            {
                logger.LogInformation("No advertisements to archive");
                return _timeBetweenJobsRun;
            }

            var archivedCount = await dbContext.Advertisements
                .Where(i => advertisementsToArchive.Contains(i.Id))
                .ExecuteUpdateAsync(update => update
                        .SetProperty(x => x.DeletedAt, dateTimeProvider.UtcNow), 
                    stoppingToken);
            
            logger.LogInformation("Moved ({Count}) advertisements to archive for {Time}", archivedCount, sw.Elapsed);
        }

        return _timeBetweenJobsRun;
    }
}