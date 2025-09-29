using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;
using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.Prediction.Abstractions;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.Extensions.Hosting;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Laraue.Apps.RealEstate.WorkerHost.Jobs;

public class UpdateAdvertisementsPredictionJob(
    IAverageRatingCalculator averageRatingCalculator,
    IAdvertisementComputedFieldsCalculator computedFieldsCalculator,
    UpdateAdvertisementsPredictionJob.IRepository repository)
    : BaseJob
{
    private const int BatchSize = 100;
    private readonly TimeSpan _jobInterval = TimeSpan.FromMinutes(5);
    
    public override async Task<TimeSpan> ExecuteAsync(
        JobState<EmptyJobData> jobState,
        CancellationToken stoppingToken = default)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nextImages = await repository.GetAdvertisementsBatchAsync(BatchSize, stoppingToken);
            if (nextImages.Count == 0)
            {
                return _jobInterval;
            }

            foreach (var image in nextImages)
            {
                var rating = averageRatingCalculator.Calculate(image.Predictions);
                var computedFieldsData = image.AdvertisementData with { RenovationRating = rating };
                var computedFields = computedFieldsCalculator.ComputeFields(computedFieldsData);
                await repository.UpdateRatingAsync(image.Id, rating, computedFields, stoppingToken);
            }
        }
        
        return _jobInterval;
    }

    public interface IRepository
    {
        Task<List<AdvertisementPredictions>> GetAdvertisementsBatchAsync(int count, CancellationToken cancellationToken = default);
        Task UpdateRatingAsync(
            long advertisementId,
            double? rating,
            ComputedFields computedFields,
            CancellationToken cancellationToken = default);
    }

    public class Repository(AdvertisementsDbContext dbContext, IDateTimeProvider dateTimeProvider) : IRepository
    {
        public Task<List<AdvertisementPredictions>> GetAdvertisementsBatchAsync(
            int count,
            CancellationToken cancellationToken = default)
        {
            return dbContext.Advertisements
                .Where(a => a.PredictedAt == null)
                .Where(a => a.LinkedImages.All(i => i.Image.PredictedAt != null))
                .OrderBy(a => a.Id)
                .Take(count)
                .Select(x => new AdvertisementPredictions
                {
                    Id = x.Id,
                    AdvertisementData = new AdvertisementData(
                        x.Square,
                        x.TotalPrice,
                        x.RenovationRating,
                        x.FloorNumber,
                        x.TotalFloorsNumber,
                        x.TransportStops.Select(s => new TransportStopData(
                            s.TransportStop!.Priority,
                            s.DistanceInMinutes,
                            s.DistanceType))),
                    Predictions = x.LinkedImages
                        .Select(i => new PredictionResult
                        {
                            RenovationRating = i.Image.RenovationRating,
                            Description = i.Image.Description,
                            Tags = i.Image.Tags,
                        })
                        .ToList()
                })
                .ToListAsyncEF(cancellationToken);
        }

        public Task UpdateRatingAsync(
            long advertisementId,
            double? rating,
            ComputedFields computedFields,
            CancellationToken cancellationToken = default)
        {
            return dbContext.Advertisements
                .Where(a => a.Id == advertisementId)
                .ExecuteUpdateAsync(upd => upd
                    .SetProperty(a => a.RenovationRating, rating)
                    .SetProperty(a => a.SquareMeterPredictedPrice, computedFields.SquareMeterPredictedPrice)
                    .SetProperty(a => a.Ideality, computedFields.Ideality)
                    .SetProperty(a => a.PredictedAt, dateTimeProvider.UtcNow), cancellationToken);
        }
    }

    public record AdvertisementPredictions
    {
        public required AdvertisementData AdvertisementData { get; init; }
        public required List<PredictionResult> Predictions { get; init; }
        public long Id { get; init; }
    }
}