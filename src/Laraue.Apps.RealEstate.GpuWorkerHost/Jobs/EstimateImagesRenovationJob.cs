using System.Diagnostics;
using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.Prediction.Abstractions;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.Extensions.Hosting;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Laraue.Apps.RealEstate.GpuWorkerHost.Jobs;

public class EstimateImagesRenovationJob(
    EstimateImagesRenovationJob.IRepository repository,
    IRemoteImagesPredictor imagesPredictor,
    ILogger<EstimateImagesRenovationJob> logger)
    : BaseJob
{
    private static readonly TimeSpan WaitUntilNextFire = TimeSpan.FromMinutes(1);
    private const int BatchSize = 10;
    
    public override async Task<TimeSpan> ExecuteAsync(
        JobState<EmptyJobData> jobState,
        CancellationToken stoppingToken = default)
    {
        var sw = new Stopwatch();
        while (!stoppingToken.IsCancellationRequested)
        {
            sw.Restart();
            
            var nextUrls = await repository.GetNextUnpredictedUrls(BatchSize, stoppingToken);
            
            logger.LogInformation("Get {ActualCount}/{Count} urls for prediction", BatchSize, nextUrls.Count);

            if (nextUrls.Count == 0)
            {
                return WaitUntilNextFire;
            }

            var predictions = await imagesPredictor.PredictAsync(nextUrls, stoppingToken);
            await repository.UpdatePredictions(predictions, stoppingToken);
            
            logger.LogInformation("Batch of {Count} urls processed for {Time}", BatchSize, sw.Elapsed);
        }
        
        return WaitUntilNextFire;
    }

    public interface IRepository
    {
        Task<List<string>> GetNextUnpredictedUrls(int count, CancellationToken ct);
        Task UpdatePredictions(IDictionary<string, PredictionResult> predictions, CancellationToken ct);
    }

    public class Repository(AdvertisementsDbContext dbContext, IDateTimeProvider dateTimeProvider) : IRepository
    {
        public Task<List<string>> GetNextUnpredictedUrls(int count, CancellationToken ct)
        {
            return dbContext.Images
                .Where(x => x.PredictedAt == null)
                .OrderBy(x => x.Id)
                .Take(count)
                .Select(x => x.Url)
                .ToListAsyncEF(ct);
        }

        public async Task UpdatePredictions(IDictionary<string, PredictionResult> predictions, CancellationToken ct)
        {
            foreach (var prediction in predictions)
            {
                await dbContext.Images
                    .Where(x => x.Url == prediction.Key)
                    .ExecuteUpdateAsync(upd => upd
                        .SetProperty(x => x.PredictedAt, dateTimeProvider.UtcNow)
                        .SetProperty(x => x.RenovationRating, prediction.Value.RenovationRating)
                        .SetProperty(x => x.Description, prediction.Value.Description)
                        .SetProperty(x => x.Tags, prediction.Value.Tags), ct);
            }
        }
    }
}