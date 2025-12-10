using System.Text;

namespace Laraue.Apps.RealEstate.Crawling.AppServices;

public static class AddressNormalizer
{
    public static string NormalizeHouseNumber(string houseNumber)
    {
        return houseNumber.Trim().ToLower();
    }
    
    public static string[] NormalizeForSearch(string houseNumber)
    {
        var houseParts = houseNumber.Split('-');
        return houseParts.Select(NormalizeSingleNumberForSearch).ToArray();
    }

    private static string NormalizeSingleNumberForSearch(string number)
    {
        number = NormalizeHouseNumber(number);
        
        if (number.Length < 1 || !char.IsDigit(number[0]))
        {
            return number;
        }

        var result = new StringBuilder();
        foreach (var character in number)
        {
            if (char.IsDigit(character))
            {
                result.Append(character);
            }
            else
            {
                break;
            }
        }
        
        return result.ToString();
    }

    private static readonly Dictionary<string, string> Replacements = new()
    {
        ["пр-т"] = "проспект",
        ["пр-кт"] = "проспект",
        ["ул."] = "улица",
        ["б-р"] = "бульвар",
        ["наб."] = "набережная",
        ["ш."] = "шоссе",
        ["пер."] = "переулок",
    };
    
    public static string NormalizeStreet(string street)
    {
        foreach (var replacement in Replacements)
        {
            street = street.Replace(replacement.Key, replacement.Value, StringComparison.InvariantCultureIgnoreCase);
        }

        return street;
    }
}