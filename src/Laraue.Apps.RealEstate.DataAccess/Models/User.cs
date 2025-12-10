using Laraue.Telegram.NET.Authentication.Models;

namespace Laraue.Apps.RealEstate.DataAccess.Models;

public sealed class User : ITelegramUser<Guid>
{
    public Guid Id { get; init; }
    public long TelegramId { get; init; }
    public string? TelegramUserName { get; init; }
    public string? TelegramLanguageCode { get; init; }
    public DateTime CreatedAt { get; init; }
}