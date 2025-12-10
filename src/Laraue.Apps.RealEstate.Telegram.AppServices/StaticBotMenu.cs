using Telegram.Bot.Types;

namespace Laraue.Apps.RealEstate.Telegram.AppServices;

public static class StaticBotMenu
{
    public const string Start = "/start";
    public const string Selections = "/selections";
    public const string Stat = "/stat";

    public static readonly IEnumerable<BotCommand> Menu = new[]
    {
        new BotCommand()
        {
            Command = Start,
            Description = "Запустить стартовое меню бота"
        },
        new()
        {
            Command = Selections,
            Description = "Посмотреть все созданные выборки"
        },
        new()
        {
            Command = Stat,
            Description = "Посмотреть статистику объявлений"
        },
    };
}