using Laraue.Apps.RealEstate.Telegram.AppServices;
using Laraue.Core.Extensions.Hosting;

namespace Laraue.Apps.RealEstate.WorkerHost.Jobs;

public class SendPublicAdvertisementsJob : BaseJob<SendPublicAdvertisementsJobContext>
{
    private readonly IAdvertisementsTelegramSender _telegramSender;

    public SendPublicAdvertisementsJob(IAdvertisementsTelegramSender telegramSender)
    {
        _telegramSender = telegramSender;
    }

    public override async Task<TimeSpan> ExecuteAsync(
        JobState<SendPublicAdvertisementsJobContext> jobState,
        CancellationToken stoppingToken = default)
    {
        var sendInterval = new TimeSpan(12, 0, 0);
        
        var sentSessionId = await _telegramSender
            .SendToPublicChannelAsync(
                jobState.JobData.LastSessionId,
                sendInterval,
                stoppingToken);

        jobState.JobData.LastSessionId = sentSessionId;

        return new TimeSpan(12, 0, 0);
    }
}

public class SendPublicAdvertisementsJobContext
{
    public long? LastSessionId { get; set; }
}