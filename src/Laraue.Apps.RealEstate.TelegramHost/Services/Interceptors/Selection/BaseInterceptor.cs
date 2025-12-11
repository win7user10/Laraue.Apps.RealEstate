using System.Linq.Expressions;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Telegram.NET.Authentication.Services;
using Laraue.Telegram.NET.Core.Extensions;
using Laraue.Telegram.NET.Core.Utils;
using Laraue.Telegram.NET.Interceptors.Services;
using LinqToDB;
using Telegram.Bot.Types.ReplyMarkups;

namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection;

public abstract class BaseInterceptor<TProperty> : BaseRequestInterceptor<Guid, TProperty, UpdateSelectionContext>, IUpdateInterceptor
{
    private readonly TelegramRequestContext<Guid> _requestContext;
    private readonly AdvertisementsDbContext _context;
    private readonly ITelegramMessageSender _messageSender;
    protected abstract Expression<Func<DataAccess.Models.Selection, TProperty?>> FieldSelectorExpression { get; }
    private Func<DataAccess.Models.Selection, TProperty?> FieldSelector => FieldSelectorExpression.Compile();

    private const string Cancel = "cancel";

    public abstract string FieldName { get; }

    public override string Id => GetType().Name;

    protected abstract Task<string> FormatTextAsync(TProperty value);

    public abstract string Description { get; }

    public virtual string? FormatDescription => null;

    protected virtual bool StayOnPreviousPageAfterEdit => false;

    public Task<string?> FormatTextAsync(DataAccess.Models.Selection value)
    {
        var propertyValue = FieldSelector(value);

        return propertyValue is null
            ? Task.FromResult((string?)null)
            : FormatTextAsync(propertyValue);
    }

    protected BaseInterceptor(
        TelegramRequestContext<Guid> requestContext,
        IInterceptorState<Guid> interceptorState,
        AdvertisementsDbContext context,
        ITelegramMessageSender messageSender)
        : base(requestContext, interceptorState)
    {
        _requestContext = requestContext;
        _context = context;
        _messageSender = messageSender;
    }

    protected override async Task ValidateAsync(
        TelegramRequestContext<Guid> requestContext,
        InterceptResult<TProperty> interceptResult,
        UpdateSelectionContext? interceptorContext)
    {
        var callbackValue = requestContext.Update.CallbackQuery?.Data;
        if (callbackValue != Cancel)
        {
            await ValidateInternalAsync(requestContext, interceptResult, interceptorContext);
            return;
        }
        
        interceptResult.Cancel();
        await _messageSender.SendSelectionEditMenuAsync(interceptorContext!.Id, _requestContext.Update.GetUserId(), null);
    }

    protected abstract Task ValidateInternalAsync(
        TelegramRequestContext<Guid> requestContext,
        InterceptResult<TProperty> interceptResult,
        UpdateSelectionContext? interceptorContext);

    protected override async Task<ExecutionState> ExecuteRouteAsync(
        TelegramRequestContext<Guid> requestContext,
        TProperty? model,
        UpdateSelectionContext? interceptorContext)
    {
        await _context
            .Selections
            .Where(x => x.Id == interceptorContext!.Id)
            .AsUpdatable()
            .Set(FieldSelectorExpression, model)
            .UpdateAsync();

        if (StayOnPreviousPageAfterEdit && interceptorContext is not null)
        {
            await _messageSender.UpdateTextAsync(
                interceptorContext.TelegramId,
                interceptorContext.MessageId,
                await GetInterceptorMessageAsync(interceptorContext));
            return ExecutionState.ParticularlyExecuted;
        }

        await _messageSender.SendSelectionEditMenuAsync(interceptorContext!.Id, _requestContext.Update.GetUserId(), null);
        return ExecutionState.FullyExecuted;
    }

    public override async Task BeforeInterceptorSetAsync(UpdateSelectionContext? context)
    {
        await _messageSender.UpdateTextAsync(
            _requestContext.Update.GetUserId(),
            _requestContext.Update.CallbackQuery.GetMessageId(),
            await GetInterceptorMessageAsync(context));
    }

    private async Task<TelegramMessageBuilder> GetInterceptorMessageAsync(UpdateSelectionContext? context)
    {
        var telegramBuilder = new TelegramMessageBuilder();

        telegramBuilder.AppendRow($"Введите значение для {FieldName}");

        telegramBuilder.AppendRow();
        telegramBuilder.AppendRow(Description);

        if (FormatDescription is not null)
        {
            telegramBuilder.AppendRow($"Формат: {FormatDescription}");
        }

        await FillMessageAsync(telegramBuilder, context);
        
        telegramBuilder.AddInlineKeyboardButtons(new []
        {
            InlineKeyboardButton.WithCallbackData("Отмена", Cancel)
        });

        return telegramBuilder;
    }

    protected virtual Task FillMessageAsync(TelegramMessageBuilder messageBuilder, UpdateSelectionContext? context)
    {
        return Task.CompletedTask;
    }
}