namespace Laraue.Apps.RealEstate.Abstractions;

public static class TelegramHostUrls
{
    public const string SelectionsUrl = "/s";
    public const string DetailSelectionUrl = $"{SelectionsUrl}/{{id}}";
    public const string CreateSelectionUrl = $"{SelectionsUrl}/new";
    public const string UpdateSelectionUrl = $"{SelectionsUrl}/{{id}}/update"; 
    public const string ViewSelectionUrl = $"{SelectionsUrl}/{{id}}/view"; 
    public const string DeleteSelectionUrl = $"{SelectionsUrl}/{{id}}/delete"; 
    public const string ViewSelectionFromNotificationUrl = $"{SelectionsUrl}/{{id}}/vfn"; 
    public const string UpdateSelectionParameterUrl = $"{SelectionsUrl}/{{id}}/update/{{interceptor}}";

    public static string GetSelectionMenuUrl(long selectionId)
    {
        return DetailSelectionUrl.ReplaceParameter("id", selectionId);
    }
    
    public static string GetUpdateSelectionUrl(long selectionId)
    {
        return UpdateSelectionUrl.ReplaceParameter("id", selectionId);
    }
    
    public static string GetViewSelectionUrl(long selectionId)
    {
        return $"{ViewSelectionUrl.ReplaceParameter("id", selectionId)}";
    }
    
    public static string GetDeleteSelectionUrl(long selectionId)
    {
        return $"{DeleteSelectionUrl.ReplaceParameter("id", selectionId)}";
    }
    
    public static string GetViewSelectionFromNotificationUrl(long selectionId)
    {
        return $"{ViewSelectionFromNotificationUrl.ReplaceParameter("id", selectionId)}";
    }
    
    public static string GetUpdateSelectionParameterUrl(long selectionId, string parameter)
    {
        return UpdateSelectionParameterUrl.ReplaceParameter("id", selectionId)
            .ReplaceParameter("interceptor", parameter);
    }

    private static string ReplaceParameter<T>(this string source, string parameter, T replacement)
    {
        return source.Replace($"{{{parameter}}}", replacement?.ToString());
    }
}