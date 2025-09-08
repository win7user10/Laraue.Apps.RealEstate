using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Core.Extensions;
using Laraue.Telegram.NET.Interceptors.Services;
using Telegram.Bot;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection;

public abstract class BaseRequestInterceptor<TInput> : BaseRequestInterceptor<Guid, TInput>
{
    private readonly TelegramRequestContext<Guid> _requestContext;
    private readonly ITelegramBotClient _client;
    
    protected BaseRequestInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        ITelegramBotClient client)
        : base(requestContext, interceptorState)
    {
        _requestContext = requestContext;
        _client = client;
    }

    public override Task BeforeInterceptorSetAsync(EmptyContext? context)
    {
        return _client.SendTextMessageAsync(
            chatId: _requestContext.Update.GetUserId(),
            text: GetBeforeSetMessage());
    }

    protected abstract string GetBeforeSetMessage();
}