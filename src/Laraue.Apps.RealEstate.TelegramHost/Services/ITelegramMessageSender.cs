using Laraue.Telegram.NET.Core.Utils;

namespace Laraue.Apps.RealEstate.TelegramHost.Services;

public interface ITelegramMessageSender
{
    Task UpdateTextAsync(long telegramId, int messageId, TelegramMessageBuilder messageBuilder, CancellationToken ct = default);
    Task SendTextAsync(long telegramId, TelegramMessageBuilder messageBuilder, CancellationToken ct = default);
    Task SendStartMenuAsync(long telegramId, CancellationToken ct = default);
    Task SendSelectionsMenuAsync(Guid userId, long telegramId, int? messageId, CancellationToken ct = default);
    Task SendSelectionMenuAsync(long selectionId, long telegramId, int messageId, CancellationToken ct = default);
    Task SendSelectionEditMenuAsync(long selectionId, long telegramId, int? messageId, CancellationToken ct = default);
    Task SendStatMenuAsync(long telegramId, CancellationToken ct = default);
    Task SendAdminStatAsync(long telegramId, CancellationToken ct = default);
}