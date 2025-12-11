using System.Linq.Expressions;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Interceptors.Services;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection;

public class UpdateSelectionNameInterceptor : BaseInterceptor<string>
{
    public UpdateSelectionNameInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
    }

    protected override Expression<Func<DataAccess.Models.Selection, string?>> FieldSelectorExpression =>
        x => x.Name;

    public override string FieldName => "Название выборки";
    protected override Task<string> FormatTextAsync(string value)
    {
        return Task.FromResult(value);
    }

    public override string Description => "Отличимое название. Показывается в списке выборок";

    protected override Task ValidateInternalAsync(TelegramRequestContext<Guid> requestContext, InterceptResult<string> interceptResult,
        UpdateSelectionContext? interceptorContext)
    {
        var text = requestContext.Update.Message?.Text;
        if (string.IsNullOrEmpty(text))
        {
            interceptResult.SetError("Название должно быть не пустое");
        }
        else
        {
            interceptResult.SetResult(text);
        }

        return Task.CompletedTask;
    }
}