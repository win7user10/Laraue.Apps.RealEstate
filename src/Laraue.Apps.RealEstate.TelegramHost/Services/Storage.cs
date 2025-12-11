using Laraue.Apps.RealEstate.Contracts;
using Laraue.Apps.RealEstate.Contracts.Extensions;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Laraue.Apps.RealEstate.TelegramHost.Services;

public class Storage : IStorage
{
    private readonly AdvertisementsDbContext _context;

    public Storage(AdvertisementsDbContext context)
    {
        _context = context;
    }

    public async Task<IList<SelectionMenuItemDto>> GetSelectionsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Selections
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Id)
            .Select(x => new SelectionMenuItemDto(x.Name, x.Id, x.NotificationInterval, x.SentAt))
            .ToListAsync(ct);
    }

    public async Task<Selection> GetSelectionSettingsAsync(long selectionId, CancellationToken ct = default)
    {
        return await _context.Selections
            .Where(x => x.Id == selectionId)
            .FirstAsync(ct);
    }

    public async Task<AdminStatDto> GetAdminStatAsync(CancellationToken ct = default)
    {
        var usersCount = await _context.Users.CountAsync(ct);
        var activeSelections = await _context.Selections
            .Where(x => x.NotificationInterval != null)
            .CountAsync(ct);

        var dailyStats = await _context.Advertisements
            .Where(x => x.UpdatedAt >= DateTime.UtcNow.AddDays(-1))
            .Where(x => x.UpdatedAt < DateTime.UtcNow)
            .GroupBy(x => x.SourceType)
            .Select(x =>
                new DailyCrawlingStatDto(x.Key, x.Count()))
            .ToListAsync(ct);
        
        var sessionsData = await _context.CrawlingSessions
            .OrderByDescending(x => x.Id)
            .Take(5)
            .Select(x => new CrawlingSessionDto(
                x.Id,
                x.StartedAt,
                x.FinishedAt,
                x.AdvertisementSource,
                x.AffectedAdvertisements.Count()))
            .ToListAsync(ct);

        return new AdminStatDto(usersCount, activeSelections, sessionsData, dailyStats);
    }

    public async Task CreateSelectionAsync(Guid userId, CancellationToken ct = default)
    {
        _context.Selections.Add(new Selection
        {
            Name = $"Выборка. Создана {DateTime.UtcNow.ToHumanReadableString()}",
            UserId = userId,
            PerPage = 2,
            NotificationInterval = new TimeSpan(12, 0, 0),
            SortBy = AdvertisementsSort.UpdatedAt,
            SortOrderBy = SortOrder.Descending
        });

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteSelectionAsync(long selectionId, CancellationToken ct = default)
    {
        await _context.Selections
            .Where(x => x.Id == selectionId)
            .ExecuteDeleteAsync(ct);
    }
}