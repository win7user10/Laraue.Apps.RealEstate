using Laraue.Apps.RealEstate.Contracts.Extensions;
using Laraue.Apps.RealEstate.Db;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Interceptors.Services;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection.Money;

public abstract class BaseMoneyInterceptor : BaseInterceptor<decimal?>
{
    protected BaseMoneyInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
    }

    protected override Task<string> FormatTextAsync(decimal? value)
    {
        return Task.FromResult(value == null ? "-" : value.Value.ToHumanReadableCurrencyString());
    }

    protected override Task ValidateInternalAsync(
        TelegramRequestContext<Guid> requestContext,
        InterceptResult<decimal?> interceptResult,
        UpdateSelectionContext? interceptorContext)
    {
        var textValue = requestContext.Update.Message?.Text;

        if (textValue == null)
        {
            interceptResult.SetResult(null);
        }
        else if (!decimal.TryParse(requestContext.Update.Message?.Text, out var value))
        {
            interceptResult.SetError("Введите числом сумму в RUB");
        }
        else if (value <= 0)
        {
            interceptResult.SetError("Введите положительное значение");
        }
        else
        {
            interceptResult.SetResult(value);
        }

        return Task.CompletedTask;
    }
}