using System.Linq.Expressions;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.Db;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Interceptors.Services;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection.Money;

public class UpdateMaxPriceInterceptor : BaseMoneyInterceptor
{
    public UpdateMaxPriceInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
    }

    public override string Id => nameof(UpdateMaxPriceInterceptor);
    protected override Expression<Func<DataAccess.Models.Selection, decimal?>> FieldSelectorExpression => selection => selection.MaxPrice;
    public override string FieldName => "Макс цена";
    public override string Description => "Значение ограничивает попадание в выборку квартир с ценой большей заданной";
}