using Laraue.Apps.RealEstate.Crawling.Contracts;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.DataAccess.Models;
using Laraue.Apps.RealEstate.Db;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.Extensions.Hosting;
using Laraue.Crawling.Crawler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Laraue.Apps.RealEstate.Crawling.AppServices;

public abstract class BaseRealEstateCrawlerJob : BaseCrawlerJob<CrawlingResult, string, State>
{
    private readonly ILogger<BaseCrawlerJob<CrawlingResult, string, State>> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly AdvertisementsDbContext _dbContext;
    private readonly IAdvertisementProcessor _processor;
    private readonly ICrawlingSchemaParser _parser;
    private readonly ISessionInterrupter _sessionInterrupter;
    private readonly BaseCrawlerServiceOptions _options;
    protected abstract string AdvertisementsAddress { get; }
    protected abstract long CityId { get; }

    protected BaseRealEstateCrawlerJob(
        ILogger<BaseCrawlerJob<CrawlingResult, string, State>> logger,
        IOptions<BaseCrawlerServiceOptions> options,
        IDateTimeProvider dateTimeProvider,
        AdvertisementsDbContext dbContext,
        IAdvertisementProcessor processor,
        ICrawlingSchemaParser parser,
        ISessionInterrupter sessionInterrupter)
        : base(logger)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _dbContext = dbContext;
        _processor = processor;
        _parser = parser;
        _sessionInterrupter = sessionInterrupter;
        _options = options.Value;
    }

    protected override Task<string> GetNextLinkAsync(JobState<State> state, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(string.Format(AdvertisementsAddress, state.JobData.LastPage));
    }

    protected override Task<CrawlingResult> ParseLinkAsync(string link, JobState<State> state, CancellationToken cancellationToken = default)
    {
        return _parser.ParseLinkAsync(link, cancellationToken);
    }

    protected override TimeSpan GetTimeToWait()
    {
        return _options.TimeBetweenSessions;
    }

    protected override async Task OnSessionStartAsync(JobState<State> state, CancellationToken cancellationToken = default)
    {
        // Session continues after the restart
        if (state.JobData.FinishedAt is null && state.JobData.StartedAt != null)
        {
            return;
        }
        
        // Reset the job state. TODO - method ShouldJobBeContinued() with auto reset state?
        state.JobData.StartedAt = _dateTimeProvider.UtcNow;
        state.JobData.FinishedAt = null;
        state.JobData.UpdatedAdvertisements = new HashSet<long>();
        state.JobData.LastPage = 1;

        var previousMaxTime = await _dbContext.Advertisements
            .Where(a => a.SourceType == _processor.Source)
            .Select(x => x.UpdatedAt)
            .DefaultIfEmpty()
            .MaxAsync(cancellationToken);

        state.JobData.MinUpdatedAt = previousMaxTime;
    }

    protected override async Task OnSessionFinishAsync(JobState<State> state, CancellationToken cancellationToken = default)
    {
        state.JobData.FinishedAt = _dateTimeProvider.UtcNow;
        
        _dbContext.CrawlingSessions.Add(new CrawlingSession
        {
            StartedAt = state.JobData.StartedAt.GetValueOrDefault(),
            FinishedAt = state.JobData.FinishedAt.GetValueOrDefault(),
            AdvertisementSource = _processor.Source,
            AffectedAdvertisements = state.JobData.UpdatedAdvertisements
                .Select(x => new CrawlingSessionAdvertisement
                {
                    AdvertisementId = x,
                })
                .ToList(),
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation(
            "Total amount of updated advs for the session: {Count}",
            state.JobData.UpdatedAdvertisements.Count);
    }

    protected override async Task AfterLinkParsedAsync(
        string link,
        CrawlingResult model,
        JobState<State> state,
        CancellationToken cancellationToken = default)
    {
        // Something went wrong. Plan to retry.
        if (!model.Advertisements.Any())
        {
            throw new CrawlerHasBeenDetectedException(
                "Advertisements are empty",
                () => Task.Delay(TimeSpan.FromMinutes(10), cancellationToken));
        }
        
        var processResult = await _processor
            .ProcessAsync(model.Advertisements, CityId, cancellationToken)
            .ConfigureAwait(false);

        _sessionInterrupter.ThrowIfRequired(
            processResult,
            state.JobData.UpdatedAdvertisements,
            _options);
        
        foreach (var updatedAdvertisementId in processResult.UpdatedAdvertisements.Keys)
        {
            state.JobData.UpdatedAdvertisements.Add(updatedAdvertisementId);
        }

        state.JobData.LastPage++;
        await UpdateStateAsync(state, cancellationToken).ConfigureAwait(false);
    }
}