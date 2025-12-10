using System.Linq.Expressions;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.Db;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Interceptors.Services;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection.Interval;

public class SelectionIntervalInterceptor : BaseIntervalInterceptor
{
    public SelectionIntervalInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
    }

    public override string Id => nameof(SelectionIntervalInterceptor);

    protected override Expression<Func<DataAccess.Models.Selection, TimeSpan?>> FieldSelectorExpression
        => selection => selection.NotificationInterval;

    public override string FieldName => "Интервал уведомлений";
    
    public override string Description =>
        "Значение указывает, как часто объявления по данной выборке должны приходить от бота пользователю.";

    public override string? FormatDescription => "чч:мм:сс (04:00:00)";
}