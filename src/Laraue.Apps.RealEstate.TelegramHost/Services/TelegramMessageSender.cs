using System.Text;
using Laraue.Apps.RealEstate.Abstractions;
using Laraue.Apps.RealEstate.Abstractions.Extensions;
using Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection;
using Laraue.Telegram.NET.Core.Extensions;
using Laraue.Telegram.NET.Core.Utils;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Laraue.Apps.RealEstate.TelegramHost.Services;

public sealed class TelegramMessageSender : ITelegramMessageSender
{
    private readonly ITelegramBotClient _client;
    private readonly UpdateInterceptorsFactory _factory;
    private readonly IStorage _storage;
    private readonly IAdvertisementStorage _advertisementStorage;
    private readonly IHealthChecker _healthChecker;

    public TelegramMessageSender(
        ITelegramBotClient client,
        UpdateInterceptorsFactory factory,
        IStorage storage,
        IAdvertisementStorage advertisementStorage,
        IHealthChecker healthChecker)
    {
        _client = client;
        _factory = factory;
        _storage = storage;
        _advertisementStorage = advertisementStorage;
        _healthChecker = healthChecker;
    }

    public Task UpdateTextAsync(long telegramId, int messageId, TelegramMessageBuilder messageBuilder,
        CancellationToken ct = default)
    {
        return _client.EditMessageTextAsync(telegramId, messageId, messageBuilder, cancellationToken: ct);
    }

    public Task SendTextAsync(long telegramId, TelegramMessageBuilder messageBuilder, CancellationToken ct = default)
    {
        return _client.SendTextMessageAsync(telegramId, messageBuilder, cancellationToken: ct);
    }

    public Task SendStartMenuAsync(long telegramId, CancellationToken ct = default)
    {
        var messageBuilder = new TelegramMessageBuilder();

        messageBuilder.AppendRow("Добро пожаловать в бот по подбору квартир");
        messageBuilder.AddInlineKeyboardButtons(new []
        {
            InlineKeyboardButton.WithCallbackData("Мои выборки", TelegramHostUrls.SelectionsUrl)
        });
        
        return _client.SendTextMessageAsync(telegramId, messageBuilder, cancellationToken: ct);
    }

    public async Task SendSelectionsMenuAsync(Guid userId, long telegramId, int? messageId, CancellationToken ct = default)
    {
        var menuItems = await _storage.GetSelectionsAsync(userId, ct);
        
        var messageBuilder = new TelegramMessageBuilder();

        messageBuilder.AppendRow(menuItems.Count == 0
            ? "У вас не настроена ни одна выборка. Добавьте хотя бы одну, чтобы просматривать объявления"
            : "Выберите выборку для просмотра объявлений");

        var i = 0;
        foreach (var menuItem in menuItems)
        {
            var sb = new StringBuilder($"{++i}) {menuItem.Name}");
            if (menuItem.NotificationInterval is not null)
            {
                var sentAt = menuItem.SentAt;
                var willSendAt = sentAt == null
                    ? DateTime.UtcNow
                    : DateTime.UtcNow.Add(-(DateTime.UtcNow - sentAt.Value));
                
                sb.Append($" - {willSendAt.ToMoscowDateTime().ToHumanReadableString()} 🔔");
            }
            
            messageBuilder.AddInlineKeyboardButtons(new []
            {
                InlineKeyboardButton.WithCallbackData(
                    sb.ToString(), TelegramHostUrls.GetSelectionMenuUrl(menuItem.Id))
            });
        }

        messageBuilder.AddInlineKeyboardButtons(new []
        {
            InlineKeyboardButton.WithCallbackData("Создать выборку", TelegramHostUrls.CreateSelectionUrl)
        });

        if (messageId.HasValue)
        {
            await _client.EditMessageTextAsync(telegramId, messageId.Value, messageBuilder, cancellationToken: ct);
        }
        else
        {
            await _client.SendTextMessageAsync(telegramId, messageBuilder, cancellationToken: ct);
        }
    }

    public async Task SendSelectionMenuAsync(long selectionId, long telegramId, int messageId, CancellationToken ct = default)
    {
        var selection = await _storage.GetSelectionSettingsAsync(selectionId, ct);
        
        var messageBuilder = new TelegramMessageBuilder();
        messageBuilder.AppendRow($"Выберите действие: {selection.Name}");
        messageBuilder.AddInlineKeyboardButtons(new[]
        {
            InlineKeyboardButton.WithCallbackData("Смотреть объявления", TelegramHostUrls.GetViewSelectionUrl(selectionId)),
            InlineKeyboardButton.WithCallbackData("Редактировать настройки", TelegramHostUrls.GetUpdateSelectionUrl(selectionId)),
        });
        
        messageBuilder.AddInlineKeyboardButtons(new[]
        {
            InlineKeyboardButton.WithCallbackData("Удалить", TelegramHostUrls.GetDeleteSelectionUrl(selectionId)),
        });

        messageBuilder.AddInlineKeyboardButtons(new[]
        {
            InlineKeyboardButton.WithCallbackData("Назад", TelegramHostUrls.SelectionsUrl),
        });
        
        await _client.EditMessageTextAsync(telegramId, messageId, messageBuilder, cancellationToken: ct);
    }

    public async Task SendSelectionEditMenuAsync(long selectionId, long telegramId, int? messageId, CancellationToken ct = default)
    {
        var settings = await _storage.GetSelectionSettingsAsync(selectionId, ct);
        
        var messageBuilder = new TelegramMessageBuilder();
        messageBuilder.AppendRow("Текущие значения")
            .AppendRow();

        var updatersByGroup = _factory.All().ToArray();
        foreach (var updater in updatersByGroup.SelectMany(x => x))
        {
            var valueText = await updater.FormatTextAsync(settings);
            if (valueText is not null)
            {
                messageBuilder.AppendRow($"<b>{updater.FieldName}</b>: {valueText}");
            }
        }

        messageBuilder.AppendRow();
        messageBuilder.AppendRow("Выберите параметр для обновления");

        var selectionsButtons = updatersByGroup
            .Select(y => y
                .Select(x =>
                {
                    var text = $"{x.FieldName}";
                    var url = TelegramHostUrls.GetUpdateSelectionParameterUrl(settings.Id, x.GetType().Name);
                    return InlineKeyboardButton.WithCallbackData(text, url);
                }));

        foreach (var selectionButtons in selectionsButtons)
        {
            messageBuilder.AddInlineKeyboardButtons(selectionButtons);
        }
        
        messageBuilder.AddInlineKeyboardButtons(new[]
        {
            InlineKeyboardButton.WithCallbackData("Назад", TelegramHostUrls.GetSelectionMenuUrl(selectionId))
        });

        if (messageId.HasValue)
        {
            await _client.EditMessageTextAsync(telegramId, messageId.Value, messageBuilder, ParseMode.Html, cancellationToken: ct);
        }
        else
        {
            await _client.SendTextMessageAsync(telegramId, messageBuilder, parseMode: ParseMode.Html, cancellationToken: ct);
        }
    }

    public async Task SendStatMenuAsync(long telegramId, CancellationToken ct = default)
    {
        var messageBuilder = new TelegramMessageBuilder();
        messageBuilder.AppendRow("Статистика по объявлениям")
            .AppendRow();

        var todayDate = DateTime.UtcNow.Date;
        var yesterdayDate = DateTime.UtcNow.AddDays(-1).Date;
        var weekAgoDate = DateTime.UtcNow.AddDays(-7).Date;
        
        var stat = await _advertisementStorage.GetMainChartAsync(
            new[]
            {
                todayDate,
                yesterdayDate,
                weekAgoDate,
            });

        var todayStat = stat.FirstOrDefault(x => x.Date == todayDate);
        var yesterdayStat = stat.FirstOrDefault(x => x.Date == yesterdayDate);
        var weekAgoStat = stat.FirstOrDefault(x => x.Date == weekAgoDate);

        if (todayStat is null || yesterdayStat is null)
        {
            messageBuilder.AppendRow("На данный момент статистика отсутствует");
        }
        else
        {
            messageBuilder.AppendRow($"Средняя цена в объявлении:")
                .AppendRow($"Сегодня: <b>{todayStat.AveragePrice.ToHumanReadableCurrencyString()}</b>")
                .AppendRow($"Вчера: <b>{yesterdayStat.AveragePrice.ToHumanReadableCurrencyString()}</b>");

            if (weekAgoStat is not null)
            {
                messageBuilder
                    .AppendRow($"Неделю назад: <b>{weekAgoStat.AveragePrice.ToHumanReadableCurrencyString()}</b>");
            }
        }
        
        await _client.SendTextMessageAsync(
            telegramId,
            messageBuilder,
            parseMode: ParseMode.Html,
            cancellationToken: ct);
    }

    public async Task SendAdminStatAsync(long telegramId, CancellationToken ct = default)
    {
        var healthResultTask = _healthChecker.CheckHealthAsync();
        var adminStat = await _storage.GetAdminStatAsync(ct);

        var tmb = new TelegramMessageBuilder();
        tmb.AppendRow("<b>Admin stat</b>");
        tmb.AppendRow();

        var healthResult = await healthResultTask;
        tmb.AppendRow($"Worker is live: <b>{healthResult.IsReady}</b>");
        if (healthResult.Error is not null)
        {
            tmb.AppendRow(healthResult.Error);
        }
        tmb.AppendRow();

        tmb.AppendRow($"Total users: <b>{adminStat.UsersCount}</b>");
        tmb.AppendRow($"Total active selections: <b>{adminStat.ActiveSelectionsCount}</b>");
        tmb.AppendRow();
        
        tmb.AppendRow($"Crawled for 24h:");
        foreach (var stat in adminStat.DailyCrawlingStats)
        {
            tmb.AppendRow($"<b>{stat.Source}</b> - {stat.AdvertisementsCount} advs");
        }

        tmb.AppendRow();
        tmb.AppendRow("Last crawling sessions:");
        foreach (var cs in adminStat.LastCrawlingSessions)
        {
            tmb.AppendRow($"<b>{cs.Id}</b>. {cs.StartedAt.ToHumanReadableString()} - {cs.FinishedAt.ToHumanReadableString()}, "+
                          $"{cs.Source:G}, {cs.AdvertisementsCount} advs");
        }
        
        await _client.SendTextMessageAsync(telegramId, tmb, parseMode: ParseMode.Html, cancellationToken: ct);
    }
}