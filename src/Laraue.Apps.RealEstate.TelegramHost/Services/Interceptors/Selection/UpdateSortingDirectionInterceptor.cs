using System.Linq.Expressions;
using Laraue.Apps.RealEstate.Contracts;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.Db;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Core.Utils;
using Laraue.Telegram.NET.Interceptors.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection;

public class UpdateSortingDirectionInterceptor : BaseInterceptor<SortOrder>
{
    private static readonly Dictionary<SortOrder, string> SortingNames = new()
    {
        [SortOrder.Ascending] = "По возрастанию",
        [SortOrder.Descending] = "По убыванию",
    };
    
    public UpdateSortingDirectionInterceptor(
        TelegramRequestContext<Guid> requestContext, 
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState, context, messageSender)
    {
    }

    public override string Id => nameof(UpdateSortingDirectionInterceptor);

    protected override Expression<Func<DataAccess.Models.Selection, SortOrder>> FieldSelectorExpression
        => x => x.SortOrderBy;

    public override string FieldName => "Направление сортировки";
    
    protected override Task<string> FormatTextAsync(SortOrder value)
    {
        return Task.FromResult(SortingNames[value]);
    }
    
    public override string Description => "Указывает, сортировка должна выполняться по возрастанию или убыванию";

    protected override Task ValidateInternalAsync(
        TelegramRequestContext<Guid> requestContext,
        InterceptResult<SortOrder> interceptResult,
        UpdateSelectionContext? interceptorContext)
    {
        var callbackData = requestContext.Update.CallbackQuery?.Data;
        if (callbackData is null || !Enum.TryParse<SortOrder>(callbackData, out var sort))
        {
            interceptResult.SetError("Необходимо выбрать один из вариантов сортировки");
            return Task.CompletedTask;
        }
        
        interceptResult.SetResult(sort);
        return Task.CompletedTask;
    }

    protected override Task FillMessageAsync(TelegramMessageBuilder messageBuilder, UpdateSelectionContext? context)
    {
        foreach (var sortsChunk in Enum.GetValues<SortOrder>().Chunk(2))
        {
            messageBuilder.AddInlineKeyboardButtons(
                sortsChunk
                    .Select(x => InlineKeyboardButton
                        .WithCallbackData(SortingNames[x], x.ToString())));
        }

        return Task.CompletedTask;
    }
}