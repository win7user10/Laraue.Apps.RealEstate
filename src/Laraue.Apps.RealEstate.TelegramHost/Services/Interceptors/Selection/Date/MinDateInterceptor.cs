using System.Linq.Expressions;
using Laraue.Apps.RealEstate.Db;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Interceptors.Services;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection.Date;

public class MinDateInterceptor : BaseDateInterceptor
{
    public MinDateInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
    }

    public override string Id => nameof(MinDateInterceptor);

    protected override Expression<Func<Db.Models.Selection, MayBeRelativeDate?>> FieldSelectorExpression
        => selection => selection.MinDate;
    public override string FieldName => "Мин. дата";

    public override string Description => "Значение ограничивает попадание в выборку объявлений с датой менее, чем установленная. " +
        "При установленном интервале даты отправки, данное значение не работает, дата высчитывается на основе интервала";
}