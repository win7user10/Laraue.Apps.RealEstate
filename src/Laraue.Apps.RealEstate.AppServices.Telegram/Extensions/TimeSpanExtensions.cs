namespace Laraue.Apps.RealEstate.AppServices.Telegram.Extensions;

public static class TimeSpanExtensions
{
    public static string ToReadableString(this TimeSpan timeSpan)
    {
        return timeSpan.Days > 0
            ? $"{timeSpan.Days}д. {timeSpan.Hours}ч. {timeSpan.Minutes}м."
            : $"{timeSpan.Hours}ч. {timeSpan.Minutes}м.";
    }
}