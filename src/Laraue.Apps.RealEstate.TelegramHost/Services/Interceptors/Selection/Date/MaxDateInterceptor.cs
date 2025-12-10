using System.Linq.Expressions;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.Db;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Interceptors.Services;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection.Date;

public class MaxDateInterceptor : BaseDateInterceptor
{
    public MaxDateInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
    }

    public override string Id => nameof(MaxDateInterceptor);

    protected override Expression<Func<DataAccess.Models.Selection, MayBeRelativeDate?>> FieldSelectorExpression
        => selection => selection.MaxDate;
    public override string FieldName => "Макс. дата";

    public override string Description => "Значение ограничивает попадание в выборку объявлений с датой более, чем установленная. " +
        "При установленном интервале даты отправки, данное значение не работает, берется дата на момент выполнения запроса";
}