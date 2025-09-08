using System.Linq.Expressions;
using Laraue.Apps.RealEstate.Db;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Interceptors.Services;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection;

public class UpdateLimitInterceptor : BaseInterceptor<int>
{
    private const string ErrorMessage = "Ожидалось число в диапазоне 1 - 10";
    
    public UpdateLimitInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
    }

    public override string Id => nameof(UpdateLimitInterceptor);
    protected override Expression<Func<Db.Models.Selection, int>> FieldSelectorExpression => x => x.PerPage;
    public override string FieldName => "Количество объявлений на странице";
    protected override Task<string> FormatTextAsync(int value) => Task.FromResult(value.ToString());
    public override string Description => "Указывает количество записей, которое должно попасть в выборку. " +
                                          "Для удобной навигации рекоменудется диапазон 1-2, чтобы список" +
                                          "объявлений целиком попадал на страницу";

    protected override Task ValidateInternalAsync(
        TelegramRequestContext<Guid> requestContext,
        InterceptResult<int> interceptResult,
        UpdateSelectionContext? interceptorContext)
    {
        var text = requestContext.Update.Message?.Text;
        if (!int.TryParse(text, out var limit) || limit < 1 || limit > 10)
        {
            interceptResult.SetError(ErrorMessage);
            return Task.CompletedTask;
        }
        
        interceptResult.SetResult(limit);
        return Task.CompletedTask;
    }
}