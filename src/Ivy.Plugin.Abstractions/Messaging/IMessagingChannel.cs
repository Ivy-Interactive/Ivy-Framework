namespace Ivy.Plugins.Messaging;

public interface IMessagingChannel
{
    string Platform { get; }
    string? DefaultChannel { get; }

    Task<MessageResult> SendMessageAsync(
        string channel,
        Message message,
        CancellationToken ct = default);

    Task DeleteMessageAsync(
        string channel,
        string messageId,
        CancellationToken ct = default);

    Task<MessageResult> UploadFileAsync(
        string channel,
        Stream content,
        string fileName,
        string? title = null,
        string? threadId = null,
        CancellationToken ct = default);
}
