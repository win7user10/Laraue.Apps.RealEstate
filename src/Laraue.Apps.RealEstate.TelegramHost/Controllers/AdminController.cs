using Laraue.Apps.RealEstate.TelegramHost.Services;
using Laraue.Telegram.NET.Authentication.Attributes;
using Laraue.Telegram.NET.Core.Extensions;
using Laraue.Telegram.NET.Core.Routing;
using Laraue.Telegram.NET.Core.Routing.Attributes;

namespace Laraue.Apps.RealEstate.TelegramHost.Controllers;

public class AdminController : TelegramController
{
    private readonly ITelegramMessageSender _sender;

    public AdminController(ITelegramMessageSender sender)
    {
        _sender = sender;
    }

    [RequiresUserRole("Admin")]
    [TelegramMessageRoute("/admin")]
    public Task SendAdminStatAsync(Request request, CancellationToken ct)
    {
        return _sender.SendAdminStatAsync(request.Update.GetUserId(), ct);
    }
}