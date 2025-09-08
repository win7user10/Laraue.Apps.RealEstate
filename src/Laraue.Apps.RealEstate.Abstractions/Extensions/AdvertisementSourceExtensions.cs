namespace Laraue.Apps.RealEstate.Abstractions.Extensions;

public static class AdvertisementSourceExtensions
{
    public static string GetAbsoluteUrl(this AdvertisementSource source)
    {
        return source switch
        {
            AdvertisementSource.Avito => "https://www.avito.ru",
            AdvertisementSource.Cian => "https://cian.ru",
            _ => throw new InvalidOperationException(),
        };
    }
    
    public static string GetAdvertisementUrl(this AdvertisementSource source, string relativeUrl)
    {
        return $"{source.GetAbsoluteUrl()}{relativeUrl}";
    }
}