using System.Text.RegularExpressions;

namespace Laraue.Apps.RealEstate.Crawling.Impl.Avito;

public static class AvitoTimeParser
{
    private static readonly Regex TimeRangeRegex = new ("(\\d+)[-|–](\\d+)[\\s]мин.", RegexOptions.Compiled);
    private static readonly Regex MaxTimeRegex = new ("до[\\s](\\d+)[\\s]мин.", RegexOptions.Compiled);
    private static readonly Regex MinTimeRegex = new ("от[\\s](\\d+)[\\s]мин.", RegexOptions.Compiled);

    public static int? Parse(string? source)
    {
        if (source is null)
        {
            return null;
        }
        
        var timeRangeMatch = TimeRangeRegex.Match(source);
        if (timeRangeMatch.Success)
        {
            return int.Parse(timeRangeMatch.Groups[2].Value);
        }
        
        var maxTimeMatch = MaxTimeRegex.Match(source);
        if (maxTimeMatch.Success)
        {
            return int.Parse(maxTimeMatch.Groups[1].Value);
        }
        
        var minTimeMatch = MinTimeRegex.Match(source);
        if (minTimeMatch.Success)
        {
            return int.Parse(minTimeMatch.Groups[1].Value);
        }

        return null;
    }
}