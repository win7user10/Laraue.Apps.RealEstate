using System.Linq.Expressions;
using Laraue.Apps.RealEstate.Db;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Interceptors.Services;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection.Money;

public class UpdateMinPriceInterceptor : BaseMoneyInterceptor
{
    public UpdateMinPriceInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
    }

    public override string Id => nameof(UpdateMinPriceInterceptor);
    protected override Expression<Func<Db.Models.Selection, decimal?>> FieldSelectorExpression => selection => selection.MinPrice;
    public override string FieldName => "Мин цена";
    public override string Description => "Значение ограничивает попадание в выборку квартир с ценой меньшей заданной";
}