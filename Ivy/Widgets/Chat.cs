using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Sender types for chat messages identifying whether message originates from user or assistant/system.</summary>
public enum ChatSender
{
    /// <summary>Message sent by user through chat interface.</summary>
    User,
    /// <summary>Message sent by assistant, bot, or system response.</summary>
    Assistant
}

/// <summary>Chat widget rendering conversations between user and assistant with message display, input handling, and conversation management.</summary>
public record Chat : WidgetBase<Chat>
{
    /// <summary>Initializes Chat with specified messages and message sending event handler.</summary>
    /// <param name="messages">Array of ChatMessage objects representing conversation history.</param>
    /// <param name="onSendMessage">Event handler called when user submits new message.</param>
    [OverloadResolutionPriority(1)]
    public Chat(ChatMessage[] messages, Func<Event<Chat, string>, ValueTask> onSendMessage) : base(messages.Cast<object>().ToArray())
    {
        OnSendMessage = onSendMessage;
        Width = Size.Full();
        Height = Size.Full();
    }

    /// <summary>Event handler called when new message is sent, enabling processing of user input and conversation management.</summary>
    [Event] public Func<Event<Chat, string>, ValueTask> OnSendMessage { get; set; }

    /// <summary>Placeholder text displayed in message input field. Default is "Type a message...".</summary>
    [Prop] public string Placeholder { get; set; } = "Type a message...";

    /// <summary>Compatibility constructor for Action-based event handlers.</summary>
    /// <param name="messages">Array of ChatMessage objects representing conversation history.</param>
    /// <param name="onSendMessage">Action-based event handler called when user submits new message.</param>
    public Chat(ChatMessage[] messages, Action<Event<Chat, string>> onSendMessage)
        : this(messages, e => { onSendMessage(e); return ValueTask.CompletedTask; })
    {
    }
}

/// <summary>Extension methods for Chat widget providing fluent API for configuring appearance and behavior.</summary>
public static class ChatExtensions
{
    /// <summary>Sets the placeholder text for the chat input field.</summary>
    /// <param name="chat">Chat widget to configure.</param>
    /// <param name="placeholder">Placeholder text to display in input field.</param>
    /// <returns>Chat instance for method chaining.</returns>
    public static Chat Placeholder(this Chat chat, string placeholder)
    {
        chat.Placeholder = placeholder;
        return chat;
    }
}

/// <summary>Single message within chat conversation containing content and sender information for structured conversation display.</summary>
public record ChatMessage : WidgetBase<ChatMessage>
{
    /// <summary>Initializes ChatMessage with specified sender and content.</summary>
    /// <param name="sender">ChatSender type indicating message origin for styling and positioning.</param>
    /// <param name="content">Content to display in the message.</param>
    public ChatMessage(ChatSender sender, object content) : base(content)
    {
        Sender = sender;
    }

    /// <summary>Sender type determining message display styling and positioning in chat interface.</summary>
    [Prop] public ChatSender Sender { get; set; }
}

/// <summary>Loading state within chat conversation indicating system processing or response generation.</summary>
public record ChatLoading : WidgetBase<ChatLoading>
{
}

/// <summary>Status message within chat conversation for system notifications, processing states, or informational messages.</summary>
public record ChatStatus : WidgetBase<ChatStatus>
{
    /// <summary>Initializes ChatStatus with specified status text.</summary>
    /// <param name="text">Status text to display describing current state or system notification.</param>
    public ChatStatus(string text)
    {
        Text = text;
    }

    /// <summary>Status text displayed in chat status message communicating current state or system notification.</summary>
    [Prop] public string Text { get; set; }
}