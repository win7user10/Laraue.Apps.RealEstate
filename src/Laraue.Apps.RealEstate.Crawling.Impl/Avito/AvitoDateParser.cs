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
    private const string JustNowPhrase = "Несколько секунд назад";
    
    public static DateTime? Parse(string? dateTimeString, IDateTimeProvider dateTimeProvider)
    {
        switch (dateTimeString)
        {
            case null:
                return null;
            case JustNowPhrase:
                return dateTimeProvider.UtcNow;
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

        if (!DateTime.TryParseExact(
            dateTimeString,
            "dd MMMM HH:mm",
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