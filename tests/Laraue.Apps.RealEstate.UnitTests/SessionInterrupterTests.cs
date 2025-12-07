using Laraue.Apps.RealEstate.Crawling.Abstractions.Contracts;
using Laraue.Apps.RealEstate.Crawling.Impl;
using Laraue.Apps.RealEstate.Crawling.Impl.Cian;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Crawling.Crawler;
using Moq;

namespace Laraue.Apps.RealEstate.UnitTests;

public class SessionInterrupterTests
{
    private readonly SessionInterrupter _sessionInterrupter;
    private readonly Mock<IDateTimeProvider> _dateTimeProvider;
    private readonly CianCrawlerServiceOptions _options = new ()
    {
        MaxAdvertisementDateOffset = new TimeSpan(1, 0, 0),
        Cities = [],
    };

    public SessionInterrupterTests()
    {
        _dateTimeProvider = new Mock<IDateTimeProvider>();
        _sessionInterrupter = new SessionInterrupter(_dateTimeProvider.Object);
    }

    [Fact]
    public void Session_ShouldBeInterrupted_WhenAdvertisementsFromPreviousSessionsFound()
    {
        var currentPageUpdatedAdvertisementIds = new HashSet<long> { 3, 4 };
        var sessionUpdatedAdvertisementIds = new HashSet<long> { 1, 2 };
        var crawledAdvertisements = Array.Empty<Advertisement>();
        
        var ex = Assert.Throws<SessionInterruptedException>(() => _sessionInterrupter.ThrowIfRequired(
            currentPageUpdatedAdvertisementIds, 
            sessionUpdatedAdvertisementIds,
            crawledAdvertisements,
            _options));
        
        Assert.Equal("Already parsed advertisements found", ex.Message);
    }

    [Fact]
    public void Session_ShouldBeInterrupted_WhenTooOldAdvertisementsFound()
    {
        var currentPageUpdatedAdvertisementIds = new HashSet<long> { 2 };
        var sessionUpdatedAdvertisementIds = new HashSet<long> { 1 };
        var crawledAdvertisements = new []
        {
            new Advertisement
            {
                UpdatedAt = new DateTime(2020, 01, 01)
            }
        };
        
        _dateTimeProvider.Setup(x => x.UtcNow)
            .Returns(new DateTime(2020, 01, 01, 02, 00, 00));
        
        var ex = Assert.Throws<SessionInterruptedException>(() => _sessionInterrupter.ThrowIfRequired(
            currentPageUpdatedAdvertisementIds, 
            sessionUpdatedAdvertisementIds,
            crawledAdvertisements,
            _options));
        
        Assert.Equal("Too old advertisements found", ex.Message);
    }
    
    [Fact]
    public void Session_ShouldNotBeInterrupted_WhenNoMatchInterruptionCriteria()
    {
        var currentPageUpdatedAdvertisementIds = new HashSet<long> { 2 };
        var sessionUpdatedAdvertisementIds = new HashSet<long> { 1 };
        var crawledAdvertisements = new []
        {
            new Advertisement
            {
                UpdatedAt = new DateTime(2020, 01, 01)
            }
        };
        
        _dateTimeProvider.Setup(x => x.UtcNow)
            .Returns(new DateTime(2020, 01, 01, 00, 10, 00));

        _sessionInterrupter.ThrowIfRequired(
            currentPageUpdatedAdvertisementIds,
            sessionUpdatedAdvertisementIds,
            crawledAdvertisements,
            _options);
    }
}