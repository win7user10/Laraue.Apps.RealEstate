using Laraue.Apps.RealEstate.Abstractions;

namespace Laraue.Apps.RealEstate.Telegram;

public interface IAdvertisementsTelegramSender
{
    /// <summary>
    /// Get advertisements for the passed selection
    /// and send them to the user the selection belongs to as a new message.
    /// Notification interval should be set for the selection to be used in this method.
    /// </summary>
    /// <returns></returns>
    Task SendFromTheJobAsync(
        long selectionId,
        CancellationToken ct = default);
    
    /// <summary>
    /// Get advertisements for the passed selection and send
    /// them to the user the selection belongs to as edit an old message.
    /// </summary>
    Task UpdateInSelectionViewAsync(
        long selectionId,
        int messageId,
        int page,
        CancellationToken ct = default);
    
    /// <summary>
    /// Get advertisements for the passed selection with overriding min and max dates and send
    /// them to the user the selection belongs to as edit an old message.
    /// </summary>
    Task UpdateInHistoryViewAsync(
        long selectionId,
        int messageId,
        int page,
        DateInterval overrideInterval,
        CancellationToken ct = default);

    /// <summary>
    /// Send a set of advertisements to the public telegram channel.
    /// </summary>
    /// <returns>Last sent session id.</returns>
    Task<long?> SendToPublicChannelAsync(
        long? previousSessionId,
        TimeSpan sendInterval,
        CancellationToken ct = default);
}