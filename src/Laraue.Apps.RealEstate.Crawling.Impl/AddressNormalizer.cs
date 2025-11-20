namespace Laraue.Apps.RealEstate.Crawling.Impl;

public static class AddressNormalizer
{
    public static string NormalizeHouseNumber(string houseNumber)
    {
        return houseNumber.Trim().ToLower();
    }
    
    public static string[] SplitHouseNumber(string houseNumber)
    {
        var houseParts = houseNumber.Split('-');
        return houseParts.Select(p => p.Trim().ToLower()).ToArray();
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