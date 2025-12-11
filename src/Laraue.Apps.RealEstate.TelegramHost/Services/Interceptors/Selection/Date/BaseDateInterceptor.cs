using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Interceptors.Services;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection.Date;

public abstract class BaseDateInterceptor : BaseInterceptor<MayBeRelativeDate?>
{
    protected BaseDateInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
    }

    public override string? FormatDescription => "дд.мм.гггг/-xd (20.12.2022/-2d)";

    protected override Task<string> FormatTextAsync(MayBeRelativeDate? value)
    {
        return Task.FromResult(value is null 
            ? "Не задано"
            : value.RelativeDaysOffset is not null
                ? $"{value.Value:d} ({value.RelativeDaysOffset} дня назад)"
                : value.Value.ToString("d"));
    }

    protected override Task ValidateInternalAsync(
        TelegramRequestContext<Guid> requestContext,
        InterceptResult<MayBeRelativeDate?> interceptResult,
        UpdateSelectionContext? interceptorContext)
    {
        var textValue = requestContext.Update.Message?.Text;
        if (textValue == null)
        {
            interceptResult.SetResult(null);
            return Task.CompletedTask;
        }

        try
        {
            interceptResult.SetResult(new MayBeRelativeDate(textValue));
        }
        catch (InvalidOperationException)
        {
            interceptResult.SetError("Необходима дата в абсолютном формате (дд.мм.гггг) или относительном (-3d)");
        }

        return Task.CompletedTask;
    }
}