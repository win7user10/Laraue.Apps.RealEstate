using System.Text.RegularExpressions;
using Laraue.Apps.RealEstate.Crawling.AppServices.Utils;
using Laraue.Apps.RealEstate.Crawling.Contracts;

namespace Laraue.Apps.RealEstate.Crawling.AppServices.Avito;

public static class AvitoTitleExtractor
{
    private static readonly Regex GetRoomsRegex = new ("(\\d+)-к.", RegexOptions.Compiled);
    private static readonly Regex GetSquareRegex = new ("([\\d|,]+)[\\s]м²", RegexOptions.Compiled);
    private static readonly Regex GetFloorsNumberRegex = new ("(\\d+)/(\\d+)[\\s]эт.", RegexOptions.Compiled);
    private const string FlatName = "квартир";
    private const string ApartmentsName = "апартамент";
    
    public static ExtractResult Extract(string? str)
    {
        var result = new ExtractResult();
        if (str is null)
        {
            return result;
        }
        
        if (str.Contains(FlatName, StringComparison.InvariantCultureIgnoreCase))
        {
            result.FlatType = FlatType.Flat;
        }
        else if (str.Contains(ApartmentsName, StringComparison.InvariantCultureIgnoreCase))
        {
            result.FlatType = FlatType.Apartments;
        }
        
        var getRoomsAndFlatTypeMatch = GetRoomsRegex.Match(str);
        if (getRoomsAndFlatTypeMatch.Success)
        {
            result.RoomsNumber = int.Parse(getRoomsAndFlatTypeMatch.Groups[1].Value);
        }
        
        var getSquareMatch = GetSquareRegex.Match(str);
        if (getSquareMatch.Success)
        {
            result.Square = decimal.Parse(getSquareMatch.Groups[1].Value, RussianCulture.Value);
        }
        
        var getFloorsMatch = GetFloorsNumberRegex.Match(str);
        if (!getFloorsMatch.Success)
        {
            return result;
        }
        
        result.Floor = int.Parse(getFloorsMatch.Groups[1].Value);
        result.TotalFloors = int.Parse(getFloorsMatch.Groups[2].Value);

        return result;
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