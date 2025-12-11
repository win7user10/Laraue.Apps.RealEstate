using System.Linq.Expressions;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Interceptors.Services;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection.Money;

public class UpdateMinSquareMeterPriceInterceptor : BaseMoneyInterceptor
{
    public UpdateMinSquareMeterPriceInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
    }

    public override string Id => nameof(UpdateMinSquareMeterPriceInterceptor);
    protected override Expression<Func<DataAccess.Models.Selection, decimal?>> FieldSelectorExpression
        => selection => selection.MinPerSquareMeterPrice;
    public override string FieldName => "Мин цена, м2";
    public override string Description => "Значение ограничивает попадание в выборку квартир с ценой за м2 меньше заданной";
}