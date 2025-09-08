namespace Laraue.Apps.RealEstate.Abstractions.Extensions;

public static class DateExtensions
{
    public static string ToHumanReadableString(this DateTime value)
    {
        return value.ToString(Constants.RusDateTimeDateFormat);
    }
    
    public static DateTime ToMoscowDateTime(this DateTime value)
    {
        return value.AddHours(3);
    }
}