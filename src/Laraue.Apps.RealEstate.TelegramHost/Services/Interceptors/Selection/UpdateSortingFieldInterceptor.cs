using System.Linq.Expressions;
using Laraue.Apps.RealEstate.Contracts;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Core.Utils;
using Laraue.Telegram.NET.Interceptors.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection;

public class UpdateSortingFieldInterceptor : BaseInterceptor<AdvertisementsSort>
{
    private static readonly Dictionary<AdvertisementsSort, string> SortingNames = new()
    {
        [AdvertisementsSort.Square] = "Площадь",
        [AdvertisementsSort.RenovationRating] = "Оценка ремонта",
        [AdvertisementsSort.RoomsCount] = "Количество комнат",
        [AdvertisementsSort.UpdatedAt] = "Дата обновления",
        [AdvertisementsSort.TotalPrice] = "Цена",
        [AdvertisementsSort.SquareMeterPrice] = "Цена за м2",
    };

    public UpdateSortingFieldInterceptor(
        TelegramRequestContext<Guid> requestContext, 
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
    }

    public override string Id => nameof(UpdateSortingFieldInterceptor);

    protected override Expression<Func<DataAccess.Models.Selection, AdvertisementsSort>> FieldSelectorExpression
        => x => x.SortBy;

    public override string FieldName => "Поле сортировки";
    
    protected override Task<string> FormatTextAsync(AdvertisementsSort value)
    {
        return Task.FromResult(SortingNames[value]);
    }
    
    public override string Description => "Указывает по какому полю должна осуществляться сортировка в выборке";

    protected override Task ValidateInternalAsync(
        TelegramRequestContext<Guid> requestContext,
        InterceptResult<AdvertisementsSort> interceptResult,
        UpdateSelectionContext? interceptorContext)
    {
        var callbackData = requestContext.Update.CallbackQuery?.Data;
        if (callbackData is null || !Enum.TryParse<AdvertisementsSort>(callbackData, out var sort))
        {
            interceptResult.SetError("Необходимо выбрать один из вариантов сортировки");
            return Task.CompletedTask;
        }
        
        interceptResult.SetResult(sort);
        return Task.CompletedTask;
    }

    protected override Task FillMessageAsync(TelegramMessageBuilder messageBuilder, UpdateSelectionContext? context)
    {
        foreach (var sortsChunk in Enum.GetValues<AdvertisementsSort>().Chunk(2))
        {
            messageBuilder.AddInlineKeyboardButtons(
                sortsChunk
                    .Select(x => InlineKeyboardButton
                        .WithCallbackData(SortingNames[x], x.ToString())));
        }

        return Task.CompletedTask;
    }
}