namespace Laraue.Apps.RealEstate.TelegramHost.Services;

public record MenuItemDto(string Name, long Id);

public sealed record SelectionMenuItemDto(string Name, long Id, TimeSpan? NotificationInterval, DateTime? SentAt)
    : MenuItemDto(Name, Id);