using System.Globalization;

namespace Laraue.Apps.RealEstate.Crawling.Impl.Utils;

public static class RussianCulture
{
    public static CultureInfo Value { get; } = new("ru-RU");
}