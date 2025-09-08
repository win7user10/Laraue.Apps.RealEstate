using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Telegram;
using Laraue.Apps.RealEstate.TelegramHost.Services;
using Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection;
using Laraue.Telegram.NET.Abstractions.Request;
using Laraue.Telegram.NET.Core.Extensions;
using Laraue.Telegram.NET.Core.Routing;
using Laraue.Telegram.NET.Core.Routing.Attributes;
using Laraue.Telegram.NET.Interceptors.Services;

namespace Laraue.Apps.RealEstate.TelegramHost.Controllers;

public class SelectionsController : TelegramController
{
    private readonly ITelegramMessageSender _messageSender;
    private readonly IAdvertisementsTelegramSender _advertisementsTelegramSender;
    private readonly IInterceptorState<Guid> _interceptorState;
    private readonly UpdateInterceptorsFactory _factory;
    private readonly IStorage _storage;

    public SelectionsController(
        ITelegramMessageSender messageSender,
        IInterceptorState<Guid> interceptorState,
        UpdateInterceptorsFactory factory,
        IAdvertisementsTelegramSender advertisementsTelegramSender,
        IStorage storage)
    {
        _messageSender = messageSender;
        _interceptorState = interceptorState;
        _factory = factory;
        _advertisementsTelegramSender = advertisementsTelegramSender;
        _storage = storage;
    }
    
    [TelegramCallbackRoute(TelegramHostUrls.SelectionsUrl)]
    public Task SendSelectionsMenuAsync(Request request)
    {
        return _messageSender.SendSelectionsMenuAsync(
            request.UserId,
            request.Update.GetUserId(),
            request.Update.CallbackQuery.GetMessageId());
    }
    
    [TelegramCallbackRoute(TelegramHostUrls.CreateSelectionUrl)]
    public async Task CreateNewSelectionAsync(Request request, CancellationToken ct)
    {
        await _storage.CreateSelectionAsync(request.UserId, ct);
        
        await _messageSender.SendSelectionsMenuAsync(
            request.UserId,
            request.Update.GetUserId(),
            null,
            ct);
    }
    
    [TelegramCallbackRoute(TelegramHostUrls.DeleteSelectionUrl)]
    public async Task DeleteSelectionAsync([FromPath] long id, Request request, CancellationToken ct)
    {
        await _storage.DeleteSelectionAsync(id, ct);
        
        await _messageSender.SendSelectionsMenuAsync(
            request.UserId,
            request.Update.GetUserId(),
            null,
            ct);
    }
    
    [TelegramCallbackRoute(TelegramHostUrls.DetailSelectionUrl)]
    public Task SendSelectionSettingsAsync([FromPath] long id, Request request)
    {
        return _messageSender.SendSelectionMenuAsync(
            id,
            request.Update.GetUserId(),
            request.Update.CallbackQuery.GetMessageId());
    }
    
    [TelegramCallbackRoute(TelegramHostUrls.UpdateSelectionUrl)]
    public Task EditSelectionAsync([FromPath] long id, Request request)
    {
        return _messageSender.SendSelectionEditMenuAsync(
            id,
            request.Update.GetUserId(),
            request.Update.CallbackQuery.GetMessageId());
    }
    
    [TelegramCallbackRoute(TelegramHostUrls.ViewSelectionUrl)]
    public Task ViewSelectionAsync([FromPath] long id, Request request, [FromQuery] int p)
    {
        return _advertisementsTelegramSender.UpdateInSelectionViewAsync(
            id,
            request.Update.CallbackQuery.GetMessageId(),
            p);
    }
    
    [TelegramCallbackRoute(TelegramHostUrls.ViewSelectionFromNotificationUrl)]
    public Task ViewSelectionAsync(
        [FromPath] long id,
        Request request,
        [FromQuery] int p,
        [FromQuery] DateTime f,
        [FromQuery] DateTime t,
        CancellationToken ct)
    {
        return _advertisementsTelegramSender.UpdateInHistoryViewAsync(
            selectionId: id,
            messageId: request.Update.CallbackQuery.GetMessageId(),
            page: p,
            overrideInterval: new DateInterval
            {
                From = f,
                To = t
            },
            ct: ct);
    }
    
    [TelegramCallbackRoute(TelegramHostUrls.UpdateSelectionParameterUrl)]
    public Task UpdateSelectionAsync([FromPath] long id, [FromPath] string interceptor, Request request)
    {
        var interceptorInstance = _factory.Get(interceptor);
        
        return _interceptorState.SetAsync(
            interceptorInstance,
            request.UserId,
            new UpdateSelectionContext(
                id,
                request.Update.GetUserId(),
                request.Update.CallbackQuery.GetMessageId()));
    }
}