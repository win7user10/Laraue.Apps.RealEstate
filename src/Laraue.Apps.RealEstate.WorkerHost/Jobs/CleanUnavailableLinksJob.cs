using System.Collections.Concurrent;
using System.Diagnostics;
using Laraue.Apps.RealEstate.DataAccess;
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
    private const int MaxParallelism = 6;
    private const int BatchSize = 150;

    public override async Task<TimeSpan> ExecuteAsync(
        JobState<EmptyJobData> jobState,
        CancellationToken stoppingToken = default)
    {
        var sw = new Stopwatch();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            sw.Restart();
            
            var imagesToCheck = await dbContext.Images
                .Where(i => i.LastAvailableAt < dateTimeProvider.UtcNow - _timeToRecheckAdvertisement)
                .OrderBy(i => i.Id)
                .Take(BatchSize)
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

            var failed = new ConcurrentBag<long>();
            var hasError = false;

            await Parallel.ForEachAsync(imagesToCheck,
                new ParallelOptions { CancellationToken = stoppingToken, MaxDegreeOfParallelism = MaxParallelism },
                async (image, ct) =>
                {
                    if (hasError)
                    {
                        return;
                    }
                    
                    try
                    {
                        if (!Uri.TryCreate(image.Url, UriKind.Absolute, out var imageUri))
                        {
                            failed.Add(image.Id);
                            return;
                        }
                        
                        var response = await client.GetAsync(imageUri, ct);
                        if (!response.IsSuccessStatusCode)
                        {
                            failed.Add(image.Id);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, $"Job takes the error requesting '{image.Url}', continue after Job restart excepted");
                        hasError = true;
                    }
                });

            if (hasError)
            {
                return TimeSpan.FromMinutes(1);
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
                "Clean images cycle finished, updated ({UpdatedCount}), deleted: ({DeletedCount}) for {Time}",
                updatedCount,
                removedCount,
                sw.Elapsed);
        }

        return _timeBetweenJobsRun;
    }
}