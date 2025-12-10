using Laraue.Apps.RealEstate.AppServices.Telegram;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.Db;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.Extensions.Hosting;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Laraue.Apps.RealEstate.WorkerHost.Jobs;

public sealed class SendSelectionsAdvertisementsJob : BaseJob
{
    private readonly IAdvertisementsTelegramSender _advertisementsTelegramSender;
    private readonly AdvertisementsDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SendSelectionsAdvertisementsJob(
        IAdvertisementsTelegramSender advertisementsTelegramSender,
        AdvertisementsDbContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        _advertisementsTelegramSender = advertisementsTelegramSender;
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }
    
    public override async Task<TimeSpan> ExecuteAsync(
        JobState<EmptyJobData> jobState,
        CancellationToken stoppingToken = default)
    {
        var utcNow = _dateTimeProvider.UtcNow;
        
        var selectionIds = await _dbContext.Selections
            .Where(x => x.NotificationInterval.HasValue)
            .Where(x => !x.SentAt.HasValue
                || (x.SentAt.HasValue && x.SentAt <= utcNow - x.NotificationInterval))
            .Select(x => x.Id)
            .ToListAsyncEF(stoppingToken);

        foreach (var selectionId in selectionIds)
        {
            await _advertisementsTelegramSender.SendFromTheJobAsync(
                selectionId: selectionId,
                ct: stoppingToken);
            
            await _dbContext.Selections
                .Where(x => x.Id == selectionId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.SentAt, utcNow), stoppingToken);
        }

        return TimeSpan.FromMinutes(5);
    }
}