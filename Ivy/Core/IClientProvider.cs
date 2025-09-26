namespace Ivy.Core;

/// <summary>
/// Interface for sending messages from server-side C# code to client-side React components.
/// </summary>
public interface IClientSender
{
    /// <summary>
    /// Sends a message with the specified method name and data to the client.
    /// </summary>
    /// <param name="method">The method name that the client will handle (e.g., "Toast", "Navigate", "CopyToClipboard").</param>
    /// <param name="data">The data payload to send with the message.</param>
    public void Send(string method, object? data);
}

/// <summary>
/// Interface providing access to client-side communication capabilities from server-side code.
/// Used for triggering client-side operations like navigation, toasts, file operations, and UI updates.
/// </summary>
public interface IClientProvider
{
    /// <summary>Gets or sets the client sender used for communication with the browser.</summary>
    public IClientSender Sender { get; set; }
}