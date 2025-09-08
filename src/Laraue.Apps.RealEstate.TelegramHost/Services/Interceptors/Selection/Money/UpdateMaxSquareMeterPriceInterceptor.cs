using System.Linq.Expressions;
using Laraue.Apps.RealEstate.Db;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Interceptors.Services;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection.Money;

public class UpdateMaxSquareMeterPriceInterceptor : BaseMoneyInterceptor
{
    public UpdateMaxSquareMeterPriceInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
    }

    public override string Id => nameof(UpdateMaxPriceInterceptor);
    protected override Expression<Func<Db.Models.Selection, decimal?>> FieldSelectorExpression => selection => selection.MaxPerSquareMeterPrice;
    public override string FieldName => "Макс цена, м2";
    public override string Description => "Значение ограничивает попадание в выборку квартир с ценой за м2 большей заданной";
}