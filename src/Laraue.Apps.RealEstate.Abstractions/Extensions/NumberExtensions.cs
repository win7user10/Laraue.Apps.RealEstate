using System.Globalization;
using System.Numerics;

namespace Laraue.Apps.RealEstate.Abstractions.Extensions;

public static class NumberExtensions
{
    public static string ToHumanReadableCurrencyString<T>(this INumber<T> value) where T : INumber<T>
    {
        return string.Format(
            new CultureInfo("ru-RU"),
            "{0:C0}",
            value);
    }
}