using System.Globalization;
using System.Text.RegularExpressions;
using Laraue.Apps.RealEstate.Crawling.Abstractions.Crawler;

namespace Laraue.Apps.RealEstate.Crawling.Impl.Cian;

public static class CianTitleExtractor
{
    private static readonly Regex GetRoomsRegex = new ("(\\d+)-к", RegexOptions.Compiled);
    private static readonly Regex GetSquareRegex = new ("(\\d+,?\\d+)[\\s]м²", RegexOptions.Compiled);
    private static readonly Regex GetFloorsNumberRegex = new ("(\\d+)/(\\d+)[\\s]эт", RegexOptions.Compiled);
    private const string FlatName = "квартир";
    private const string ApartmentsName = "апартамент";

    private static readonly CultureInfo RussianCulture = new ("ru-RU");

    public static ExtractResult ExtractTitle(string? title, string? subTitle)
    {
        var result = new ExtractResult();
        
        EnrichResult(result, subTitle);
        EnrichResult(result, title);

        return result;
    }

    private static void EnrichResult(ExtractResult result, string? title)
    {
        if (title is null)
        {
            return;
        }
        
        if (title.Contains(FlatName, StringComparison.InvariantCultureIgnoreCase))
        {
            result.FlatType = FlatType.Flat;
        }
        else if (title.Contains(ApartmentsName, StringComparison.InvariantCultureIgnoreCase))
        {
            result.FlatType = FlatType.Apartments;
        }
        
        var getRoomsAndFlatTypeMatch = GetRoomsRegex.Match(title);
        if (getRoomsAndFlatTypeMatch.Success)
        {
            result.RoomsNumber = int.Parse(getRoomsAndFlatTypeMatch.Groups[1].Value);
        }
        
        var getSquareMatch = GetSquareRegex.Match(title);
        if (getSquareMatch.Success)
        {
            var decimalSeparator = getSquareMatch.Groups[3].Value;
            result.Square = decimal.Parse(
                getSquareMatch.Groups[1].Value,
                decimalSeparator == "." ? CultureInfo.InvariantCulture : RussianCulture);
        }
        
        var getFloorsMatch = GetFloorsNumberRegex.Match(title);
        if (!getFloorsMatch.Success)
        {
            return;
        }
        
        result.Floor = int.Parse(getFloorsMatch.Groups[1].Value);
        result.TotalFloors = int.Parse(getFloorsMatch.Groups[2].Value);
    }

    public sealed record ExtractResult
    {
        public int? RoomsNumber { get; set; }
        public decimal? Square { get; set; }
        public int? Floor { get; set; }
        public int? TotalFloors { get; set; }
        public FlatType FlatType { get; set; }
    }
}