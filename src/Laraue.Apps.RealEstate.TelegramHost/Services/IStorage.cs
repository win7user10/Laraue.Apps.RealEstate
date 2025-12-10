using Laraue.Apps.RealEstate.DataAccess.Models;

namespace Laraue.Apps.RealEstate.TelegramHost.Services;

public interface IStorage
{
    Task<IList<SelectionMenuItemDto>> GetSelectionsAsync(Guid userId, CancellationToken ct = default);
    
    Task<Selection> GetSelectionSettingsAsync(long selectionId, CancellationToken ct = default);
    
    Task<AdminStatDto> GetAdminStatAsync(CancellationToken ct = default);
    
    Task CreateSelectionAsync(Guid userId, CancellationToken ct = default);
    
    Task DeleteSelectionAsync(long selectionId, CancellationToken ct = default);
}