using System.Text.RegularExpressions;

namespace Laraue.Apps.RealEstate.Db;

public sealed class MayBeRelativeDate
{
    private static readonly Regex RelativeDateTimeRegex = new ("\\-(\\d{1,2})d", RegexOptions.Compiled);

    public string SourceValue { get; }

    public DateTime Value { get; }

    public int? RelativeDaysOffset { get; }

    public MayBeRelativeDate(string value)
    {
        var matchRelativeDateTime = RelativeDateTimeRegex.Match(value);
        if (matchRelativeDateTime.Success)
        {
            RelativeDaysOffset = int.Parse(matchRelativeDateTime.Groups[1].Value);
            
            Value = DateTime.UtcNow.AddDays(-RelativeDaysOffset.Value);
        }
        else if (DateTime.TryParse(value, out var dateTime))
        {
            Value = dateTime;
        }
        else
        {
            throw new InvalidOperationException($"Invalid relative date {value}");
        }

        SourceValue = value;
    }

    public override string ToString()
    {
        return SourceValue;
    }
}