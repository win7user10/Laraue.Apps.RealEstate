using Laraue.Apps.RealEstate.AppServices.Extensions;
using Laraue.Apps.RealEstate.Contracts;
using Laraue.Apps.RealEstate.Contracts.Extensions;
using Laraue.Apps.RealEstate.DataAccess;
using Laraue.Apps.RealEstate.DataAccess.Models;
using Laraue.Apps.RealEstate.Telegram.AppServices.Extensions;
using Laraue.Core.DataAccess.Contracts;
using Laraue.Core.DateTime.Services.Abstractions;
using Laraue.Telegram.NET.Core.Extensions;
using Laraue.Telegram.NET.Core.Routing;
using Laraue.Telegram.NET.Core.Utils;
using Laraue.Telegram.NET.DataAccess.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Laraue.Apps.RealEstate.Telegram.AppServices;

public sealed class AdvertisementsTelegramSender : IAdvertisementsTelegramSender
{
    private readonly ITelegramBotClient _botClient;
    private readonly AdvertisementsSenderOptions _options;
    private readonly IAdvertisementService _service;
    private readonly AdvertisementsDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    private const string DateFormat = "yyyy-MM-ddTHH:mm:ssZ";
    private string? _botUsername;

    public AdvertisementsTelegramSender(
        ITelegramBotClient botClient,
        IOptions<AdvertisementsSenderOptions> options,
        IAdvertisementService service,
        AdvertisementsDbContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        _botClient = botClient;
        _service = service;
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _options = options.Value;
    }

    public Task SendFromTheJobAsync(long selectionId, CancellationToken ct = default)
    {
        return SendAsync(
            selectionId: selectionId,
            getRequest: selection => selection.ToAdvertisementsRequest(
                getMinDateFromNotificationInterval: true),
            getMessageBuilder: (selection, result, request) => GetMessage(
                request.Filter.MinDate.GetValueOrDefault(),
                request.Filter.MaxDate.GetValueOrDefault(),
                selection.Name,
                result,
                new CallbackRoutePath(TelegramHostUrls.GetViewSelectionFromNotificationUrl(selectionId))
                    .WithQueryParameter("f", request.Filter.MinDate?.ToString(DateFormat))
                    .WithQueryParameter("t", request.Filter.MaxDate?.ToString(DateFormat))),
            messageId: null,
            ct: ct);
    }

    public Task UpdateInHistoryViewAsync(
        long selectionId,
        int messageId,
        int page,
        DateInterval overrideInterval,
        CancellationToken ct = default)
    {
        return SendAsync(
            selectionId: selectionId,
            getRequest: selection => selection.ToAdvertisementsRequest(
                minDate: overrideInterval.From,
                maxDate: overrideInterval.To,
                page: page),
            getMessageBuilder: (selection, result, _) => GetMessage(
                overrideInterval.From,
                overrideInterval.To,
                selection.Name,
                result,
                new CallbackRoutePath(TelegramHostUrls.GetViewSelectionFromNotificationUrl(selectionId))
                    .WithQueryParameter("f", overrideInterval.From.ToString(DateFormat))
                    .WithQueryParameter("t", overrideInterval.To.ToString(DateFormat))),
            messageId: messageId,
            ct: ct);
    }

    public async Task<long?> SendToPublicChannelAsync(
        long? previousSessionId,
        TimeSpan sendInterval,
        CancellationToken ct = default)
    {
        var minValue = DateTime.MinValue;
        if (previousSessionId is not null)
        {
            var previousSession = await _dbContext.CrawlingSessions
                .Where(x => x.Id == previousSessionId)
                .FirstAsync(ct);

            minValue = previousSession.FinishedAt;
        }
        
        var lastSession = await _dbContext.CrawlingSessions
            .OrderByDescending(x => x.FinishedAt)
            .FirstOrDefaultAsync(ct);

        var maxDate = lastSession?.FinishedAt ?? _dateTimeProvider.UtcNow;
        
        var request = new AdvertisementsRequest
        {
            Filter = new Filter
            {
                MaxDate = maxDate,
                MinDate = minValue,
                MinRenovationRating = 7,
                SortBy = AdvertisementsSort.UpdatedAt,
                SortOrderBy = SortOrder.Descending,
                MinPrice = 5_000_000,
                MaxPrice = 9_000_000,
            },
            Pagination = new PaginationData
            {
                PerPage = 3
            },
        };

        var botUsername = await GetBotNameAsync(ct);

        await SendAsync(
            telegramId: _options.ChatId,
            request: request,
            getMessageBuilder: (result, advertisementsRequest) =>
            {
                var messageBuilder = GetMessage(
                    intervalTitle: $"За последние {sendInterval.ToReadableString()}",
                    categoryTitle: $"от {advertisementsRequest.Filter.MinPrice?.ToHumanReadableCurrencyString()} " +
                                   $"до {advertisementsRequest.Filter.MaxPrice?.ToHumanReadableCurrencyString()}",
                    result: result.Data);

                messageBuilder.AppendRow();
                messageBuilder.AppendRow($"<i>Индивидуальная настройка подборки объявлений в боте {botUsername}</i>");

                return messageBuilder;
            },
            messageId: null,
            ct: ct);

        return lastSession?.Id;
    }

    public Task UpdateInSelectionViewAsync(
        long selectionId,
        int messageId,
        int page,
        CancellationToken ct = default)
    {
        return SendAsync(
            selectionId: selectionId,
            getRequest: selection => selection.ToAdvertisementsRequest(
                getMinDateFromNotificationInterval: true,
                page: page),
            getMessageBuilder: (selection, result, _) =>
            {
                var builder = GetMessage(
                    selection.NotificationInterval ??
                    throw new InvalidOperationException(
                        "Selection should have the interval set to be send from the job"),
                    selection.Name,
                    result,
                    new CallbackRoutePath(TelegramHostUrls.GetViewSelectionUrl(selectionId)));

                return builder.AddInlineKeyboardButtons(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", TelegramHostUrls.GetSelectionMenuUrl(selectionId))
                });
            },
            messageId: messageId,
            ct: ct);
    }

    private async ValueTask<string> GetBotNameAsync(CancellationToken cancellationToken)
    {
        if (_botUsername is not null)
        {
            return _botUsername;
        }
        
        var me = await _botClient.GetMe(cancellationToken);
        _botUsername = $"@{me.Username}";

        return _botUsername!;
    }

    private TelegramMessageBuilder GetMessage(
        DateTime minDate,
        DateTime maxDate,
        string selectionName,
        IShortPaginatedResult<AdvertisementDto> result,
        CallbackRoutePath route)
    {
        return GetMessage(
            intervalTitle: $"Даты {minDate.ToHumanReadableString()} - {maxDate.ToHumanReadableString()}",
            selectionName: selectionName,
            result: result,
            route: route);
    }
    
    private TelegramMessageBuilder GetMessage(
        TimeSpan interval,
        string selectionName,
        IShortPaginatedResult<AdvertisementDto> result,
        CallbackRoutePath route)
    {
        return GetMessage(
            intervalTitle: $"Последние {interval.ToReadableString()}",
            selectionName: selectionName,
            result: result,
            route: route);
    }
    
    private TelegramMessageBuilder GetMessage(
        string intervalTitle,
        string selectionName,
        IShortPaginatedResult<AdvertisementDto> result,
        CallbackRoutePath route)
    {
        var messageBuilder = new TelegramMessageBuilder();
        messageBuilder.AppendRow($"Выборка: <b>{selectionName}</b>");
        messageBuilder.AppendRow($"Интервал: <b>{intervalTitle}</b>");
        messageBuilder.AppendRow();
        
        messageBuilder.Append(result.Data.ToTelegramString());
        
        messageBuilder.AddPaginationButtons(
            result: result,
            route,
            previousButtonText: "Предыдущая ⬅",
            nextButtonText: "Следующая ➡");

        return messageBuilder;
    }
    
    private TelegramMessageBuilder GetMessage(
        string intervalTitle,
        string categoryTitle,
        IEnumerable<AdvertisementDto> result)
    {
        var messageBuilder = new TelegramMessageBuilder();
        messageBuilder.AppendRow($"Интересные квартиры: <b>{intervalTitle}</b>");
        messageBuilder.AppendRow($"Категория: <b>{categoryTitle}</b>");
        messageBuilder.AppendRow();
        
        messageBuilder.Append(result.ToTelegramString());

        return messageBuilder;
    }
    
    private async Task SendAsync(
        ChatId telegramId,
        AdvertisementsRequest request,
        Func<IShortPaginatedResult<AdvertisementDto>, AdvertisementsRequest, TelegramMessageBuilder> getMessageBuilder,
        int? messageId,
        CancellationToken ct)
    {
        var result = await _service.GetAdvertisementsAsync(request);
        if (!result.Data.Any())
        {
            return;
        }

        var messageBuilder = getMessageBuilder(result, request);

        if (!messageId.HasValue)
        {
            await _botClient.SendTextMessageAsync(
                telegramId,
                messageBuilder,
                parseMode: ParseMode.Html,
                cancellationToken: ct);
        }
        else
        {
            await _botClient.EditMessageTextAsync(
                telegramId,
                messageId.Value,
                messageBuilder,
                parseMode: ParseMode.Html,
                cancellationToken: ct);
        }
    }

    private async Task SendAsync(
        long selectionId,
        Func<Selection, AdvertisementsRequest> getRequest,
        Func<Selection, IShortPaginatedResult<AdvertisementDto>, AdvertisementsRequest, TelegramMessageBuilder> getMessageBuilder,
        int? messageId,
        CancellationToken ct)
    {
        var selection = await _dbContext.Selections
            .Where(x => x.Id == selectionId)
            .Include(x => x.User)
            .FirstAsync(ct);

        var telegramId = selection.User?.TelegramId;
        if (!telegramId.HasValue)
        {
            return;
        }
        
        var request = getRequest(selection);
        
        await SendAsync(
            telegramId: telegramId.Value,
            request: request,
            getMessageBuilder: (result, r) => getMessageBuilder(selection, result, r),
            messageId,
            ct);
    }
}