using System.Globalization;
using System.Text.RegularExpressions;
using Laraue.Apps.RealEstate.Crawling.Impl.Utils;
using Laraue.Core.DateTime.Extensions;
using Laraue.Core.DateTime.Services.Abstractions;

namespace Laraue.Apps.RealEstate.Crawling.Impl.Avito;

public static class AvitoDateParser
{
    private static readonly Regex MinutesRegex = new ("(\\d+) минут(ы|у)? назад", RegexOptions.Compiled);
    private static readonly Regex HoursRegex = new ("(\\d+) час(а|ов)? назад", RegexOptions.Compiled);
    private static readonly Regex DaysRegex = new ("(\\d+) (день|дней|дня)? назад", RegexOptions.Compiled);
    private static readonly Regex WeeksRegex = new ("(\\d+) (неделю|недели)? назад", RegexOptions.Compiled);
    private const string JustNowPhrase = "Несколько секунд назад";
    private const string YesterdayPhrase = "Вчера";
    
    public static DateTime? Parse(string? dateTimeString, IDateTimeProvider dateTimeProvider)
    {
        switch (dateTimeString)
        {
            case null:
                return null;
            case JustNowPhrase:
                return dateTimeProvider.UtcNow;
            case YesterdayPhrase:
                return dateTimeProvider.UtcNow.AddDays(-1).StartOfDay();
        }

        var minutesRegexResult = MinutesRegex.Match(dateTimeString);
        if (minutesRegexResult.Success)
        {
            var minutes = int.Parse(minutesRegexResult.Groups[1].Value);
            return dateTimeProvider.UtcNow.AddMinutes(-minutes).StartOfMinute();
        }
        
        var hoursRegexResult = HoursRegex.Match(dateTimeString);
        if (hoursRegexResult.Success)
        {
            var hours = int.Parse(hoursRegexResult.Groups[1].Value);
            return dateTimeProvider.UtcNow.AddHours(-hours).StartOfHour();
        }
        
        var daysRegexResult = DaysRegex.Match(dateTimeString);
        if (daysRegexResult.Success)
        {
            var days = int.Parse(daysRegexResult.Groups[1].Value);
            return dateTimeProvider.UtcNow.AddDays(-days).StartOfDay();
        }
        
        var weeksRegexResult = WeeksRegex.Match(dateTimeString);
        if (weeksRegexResult.Success)
        {
            var weeks = int.Parse(weeksRegexResult.Groups[1].Value);
            return dateTimeProvider.UtcNow.AddDays(-weeks * 7).StartOfDay();
        }

        if (!DateTime.TryParseExact(
            dateTimeString,
            "d MMMM HH:mm",
            RussianCulture.Value,
            DateTimeStyles.None,
            out var date))
        {
            return null;
        }
        
        var currentDate = dateTimeProvider.UtcNow;
        return new DateTime(currentDate.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
    }
}