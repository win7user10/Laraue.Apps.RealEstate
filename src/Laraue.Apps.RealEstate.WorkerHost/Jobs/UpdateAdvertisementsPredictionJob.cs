using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.Prediction.Abstractions;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.Extensions.Hosting;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Laraue.Apps.RealEstate.WorkerHost.Jobs;

public class UpdateAdvertisementsPredictionJob(
    IAdvertisementComputedFieldsCalculator computedFieldsCalculator,
    UpdateAdvertisementsPredictionJob.IRepository repository,
    ILogger<UpdateAdvertisementsPredictionJob> logger)
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
            var nextAdvertisements = await repository.GetAdvertisementsBatchAsync(BatchSize, stoppingToken);
            if (nextAdvertisements.Count == 0)
            {
                return _jobInterval;
            }

            foreach (var advertisement in nextAdvertisements)
            {
                var computedFields = computedFieldsCalculator.ComputeFields(advertisement.AdvertisementData);
                await repository.UpdateRatingAsync(advertisement.Id, computedFields, stoppingToken);
                logger.LogInformation("Adv id:{Id} is ready for API", advertisement.Id);
            }
            
            logger.LogInformation("Prediction updated for the batch of {Count} advertisements", nextAdvertisements.Count);
        }
        
        return _jobInterval;
    }

    public interface IRepository
    {
        Task<List<AdvertisementPredictions>> GetAdvertisementsBatchAsync(int count, CancellationToken cancellationToken = default);
        Task UpdateRatingAsync(
            long advertisementId,
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
                .Where(a => a.ReadyAt == null && a.PredictedAt != null)
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
                })
                .ToListAsyncEF(cancellationToken);
        }

        public Task UpdateRatingAsync(
            long advertisementId,
            ComputedFields computedFields,
            CancellationToken cancellationToken = default)
        {
            return dbContext.Advertisements
                .Where(a => a.Id == advertisementId)
                .ExecuteUpdateAsync(upd => upd
                    .SetProperty(a => a.ReadyAt, dateTimeProvider.UtcNow), cancellationToken);
        }
    }

    public record AdvertisementPredictions
    {
        public required AdvertisementData AdvertisementData { get; init; }
        public long Id { get; init; }
    }
}