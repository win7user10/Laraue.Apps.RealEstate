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
    
    public override async Task<TimeSpan> ExecuteAsync(
        JobState<EmptyJobData> jobState,
        CancellationToken stoppingToken = default)
    {
        var sw = new Stopwatch();
        while (!stoppingToken.IsCancellationRequested)
        {
            sw.Restart();
            
            var dataToPredict = await repository.GetNextUnpredictedAdvertisement(stoppingToken);
            if (dataToPredict is null)
            {
                return WaitUntilNextFire;
            }
            
            logger.LogInformation("Predict id:{Id} ({Count}) images", dataToPredict.Id, dataToPredict.ImageUrls.Count);

            var prediction = await imagesPredictor.PredictAsync(
                dataToPredict.ImageUrls,
                stoppingToken);
            
            await repository.UpdatePrediction(dataToPredict.Id, prediction, stoppingToken);
            
            logger.LogInformation("id:{Id} is predicted for {Time}", dataToPredict.Id, sw.Elapsed);
        }
        
        return WaitUntilNextFire;
    }

    public interface IRepository
    {
        Task<AdvertisementPredictionData?> GetNextUnpredictedAdvertisement(CancellationToken ct);
        Task UpdatePrediction(long id, PredictionResult prediction, CancellationToken ct);
    }

    public class Repository(AdvertisementsDbContext dbContext, IDateTimeProvider dateTimeProvider) : IRepository
    {
        public Task<AdvertisementPredictionData?> GetNextUnpredictedAdvertisement(CancellationToken ct)
        {
            return dbContext.Advertisements
                .Where(x => x.PredictedAt == null)
                .Select(x => new AdvertisementPredictionData
                {
                    Id = x.Id,
                    ImageUrls = x.LinkedImages.Select(y => y.Image.Url).ToArray()
                })
                .FirstOrDefaultAsyncEF(ct);
        }

        public async Task UpdatePrediction(long id, PredictionResult prediction, CancellationToken ct)
        {
            await dbContext.Advertisements
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(upd => upd
                    .SetProperty(x => x.PredictedAt, dateTimeProvider.UtcNow)
                    .SetProperty(x => x.RenovationRating, prediction.RenovationRating)
                    .SetProperty(x => x.Advantages, prediction.Advantages)
                    .SetProperty(x => x.Problems, prediction.Problems),
                    ct);
        }
    }

    public class AdvertisementPredictionData
    {
        public long Id { get; set; }
        public required IList<string> ImageUrls { get; set; }
    }
}