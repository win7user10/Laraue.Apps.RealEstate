using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Interceptors.Services;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection.Interval;

public abstract class BaseIntervalInterceptor : BaseInterceptor<TimeSpan?>
{
    private const string ErrorMessage = "Необходимо ввести временной интервал в формате чч:мм:сс";
    
    protected BaseIntervalInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
    }
    
    protected override Task<string> FormatTextAsync(TimeSpan? value)
    {
        return Task.FromResult(value.ToString() ?? "Не задан");
    }

    protected override Task ValidateInternalAsync(
        TelegramRequestContext<Guid> requestContext,
        InterceptResult<TimeSpan?> interceptResult,
        UpdateSelectionContext? interceptorContext)
    {
        var textValue = requestContext.Update.Message?.Text;
        if (textValue == null)
        {
            interceptResult.SetError(ErrorMessage);
            return Task.CompletedTask;
        }

        if (TimeSpan.TryParse(textValue, out var timeSpan))
        {
            interceptResult.SetResult(timeSpan);
        }
        else
        {
            interceptResult.SetError(ErrorMessage);
        }

        return Task.CompletedTask;
    }
}