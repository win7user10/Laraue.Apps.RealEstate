using Laraue.Apps.RealEstate.Telegram;
using Laraue.Apps.RealEstate.TelegramHost.Services;
using Laraue.Telegram.NET.Core.Extensions;
using Laraue.Telegram.NET.Core.Routing;
using Laraue.Telegram.NET.Core.Routing.Attributes;

namespace Laraue.Apps.RealEstate.TelegramHost.Controllers;

public class StaticMenuController : TelegramController
{
    private readonly ITelegramMessageSender _sender;

    public StaticMenuController(ITelegramMessageSender sender)
    {
        _sender = sender;
    }

    [TelegramMessageRoute(StaticBotMenu.Start)]
    public Task StartBotAsync(Request request, CancellationToken ct)
    {
        return _sender.SendStartMenuAsync(request.Update.GetUserId(), ct);
    }
    
    [TelegramMessageRoute(StaticBotMenu.Selections)]
    public Task SendSelectionsAsync(Request request, CancellationToken ct)
    {
        return _sender.SendSelectionsMenuAsync(
            request.UserId,
            request.Update.GetUserId(),
            messageId: null,
            ct);
    }
    
    [TelegramMessageRoute(StaticBotMenu.Stat)]
    public Task SendStatAsync(Request request, CancellationToken ct)
    {
        return _sender.SendStatMenuAsync(
            request.Update.GetUserId(),
            ct);
    }
}