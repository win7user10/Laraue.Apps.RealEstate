using Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;
using Laraue.Apps.RealEstate.Db;
using Laraue.Apps.RealEstate.Db.Models;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Core.Extensions.Hosting;
using Laraue.Crawling.Crawler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Advertisement = Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts.Advertisement;

namespace Laraue.Apps.RealEstate.Crawling.Impl;

public abstract class BaseRealEstateCrawlerJob : BaseCrawlerJob<CrawlingResult, string, State>
{
    private readonly ILogger<BaseCrawlerJob<CrawlingResult, string, State>> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly AdvertisementsDbContext _dbContext;
    private readonly IAdvertisementProcessor _processor;
    private readonly ICrawlingSchemaParser _parser;
    private readonly BaseCrawlerServiceOptions _options;
    protected abstract string AdvertisementsAddress { get; }

    protected BaseRealEstateCrawlerJob(
        ILogger<BaseCrawlerJob<CrawlingResult, string, State>> logger,
        IOptions<BaseCrawlerServiceOptions> options,
        IDateTimeProvider dateTimeProvider,
        AdvertisementsDbContext dbContext,
        IAdvertisementProcessor processor,
        ICrawlingSchemaParser parser)
        : base(logger)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _dbContext = dbContext;
        _processor = processor;
        _parser = parser;
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

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            // TODO - handle only constraint exceptions here
        }
        
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
        var updatedAdvertisementIds = await _processor
            .ProcessAsync(model.Advertisements, cancellationToken)
            .ConfigureAwait(false);
            
        foreach (var updatedAdvertisementId in updatedAdvertisementIds)
        {
            state.JobData.UpdatedAdvertisements.Add(updatedAdvertisementId);
        }

        if (!model.Advertisements.Any())
        {
            throw new CrawlerHasBeenDetectedException(
                "Advertisements are empty",
                () => Task.Delay(TimeSpan.FromMinutes(10), cancellationToken));
        }
        
        if (model.Advertisements.Min(x => x.UpdatedAt) < state.JobData.MinUpdatedAt)
        {
            throw new SessionInterruptedException("Already parsed advertisements found");
        }

        if (!AreAllAdvertisementsActual(model.Advertisements))
        {
            throw new SessionInterruptedException("Too old advertisements found");
        }

        state.JobData.LastPage++;
        await UpdateStateAsync(state, cancellationToken).ConfigureAwait(false);
    }
    
    private bool AreAllAdvertisementsActual(IEnumerable<Advertisement> advertisements)
    {
        var latestAdvertisementDate = advertisements.Max(x => x.UpdatedAt);
        
        return _dateTimeProvider.UtcNow - latestAdvertisementDate
               <= _options.MaxAdvertisementDateOffset;
    }
}