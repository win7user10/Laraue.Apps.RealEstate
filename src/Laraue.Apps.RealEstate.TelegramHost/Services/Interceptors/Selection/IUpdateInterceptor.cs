namespace Laraue.Apps.RealEstate.TelegramHost.Services.Interceptors.Selection;

public interface IUpdateInterceptor
{
    /// <summary>
    /// Field name to show it in the message.
    /// </summary>
    string FieldName { get; }
    
    /// <summary>
    /// Get field value from the selection and format it to readable text.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task<string?> FormatTextAsync(Db.Models.Selection value);

    /// <summary>
    /// Field description for the user.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Input format description for the user.
    /// </summary>
    string? FormatDescription { get; }
}