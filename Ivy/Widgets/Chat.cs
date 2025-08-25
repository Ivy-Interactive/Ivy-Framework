using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines the sender types for chat messages, identifying whether a message
/// originates from the user or from an assistant/system response.
/// </summary>
public enum ChatSender
{
    /// <summary>Message sent by the user through the chat interface.</summary>
    User,
    /// <summary>Message sent by an assistant, bot, or system response.</summary>
    Assistant
}

/// <summary>
/// Represents a chat widget that renders a conversation between a user and
/// an assistant. This widget provides a complete chat interface with message
/// display, input handling, and conversation management capabilities.
/// 
/// The Chat widget supports various message types including text, rich content,
/// loading states, and interactive elements, making it ideal for creating
/// conversational interfaces, AI assistants, customer support systems, and
/// any application requiring real-time communication between users and systems.
/// </summary>
public record Chat : WidgetBase<Chat>
{
    /// <summary>
    /// Initializes a new instance of the Chat class with the specified messages
    /// and message sending event handler. The chat will display the provided
    /// conversation history and handle new message submissions through the
    /// specified event handler.
    /// </summary>
    /// <param name="messages">Array of ChatMessage objects representing the
    /// conversation history to display in the chat interface. These messages
    /// will be rendered in chronological order with appropriate sender styling.</param>
    /// <param name="onSendMessage">Event handler that is called when the user
    /// submits a new message. This handler receives the chat event context and
    /// the message text, allowing you to process the message and update the
    /// conversation accordingly.</param>
    [OverloadResolutionPriority(1)]
    public Chat(ChatMessage[] messages, Func<Event<Chat, string>, ValueTask> onSendMessage) : base(messages.Cast<object>().ToArray())
    {
        OnSendMessage = onSendMessage;
        Width = Size.Full();
        Height = Size.Full();
    }

    /// <summary>
    /// Gets or sets the event handler that is called when a new message is sent.
    /// This event handler receives the chat event context and the message text,
    /// enabling you to process user input, generate responses, and manage
    /// the conversation flow dynamically.
    /// 
    /// The event handler is responsible for updating the message list, processing
    /// user input, generating appropriate responses, and managing any business
    /// logic related to the chat conversation.
    /// </summary>
    [Event] public Func<Event<Chat, string>, ValueTask> OnSendMessage { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text displayed in the message input field.
    /// This property provides guidance to users about what type of input is
    /// expected, improving the user experience by making the interface more
    /// intuitive and user-friendly.
    /// 
    /// The placeholder text is displayed when the input field is empty and
    /// disappears when the user begins typing, providing contextual guidance
    /// without cluttering the interface.
    /// Default is "Type a message...".
    /// </summary>
    [Prop] public string Placeholder { get; set; } = "Type a message...";

    /// <summary>
    /// Compatibility constructor for Action-based event handlers.
    /// Automatically wraps Action delegates in ValueTask-returning functions for backward compatibility.
    /// </summary>
    /// <param name="messages">Array of ChatMessage objects representing the conversation history to display.</param>
    /// <param name="onSendMessage">Action-based event handler that is called when the user submits a new message.</param>
    public Chat(ChatMessage[] messages, Action<Event<Chat, string>> onSendMessage)
        : this(messages, e => { onSendMessage(e); return ValueTask.CompletedTask; })
    {
    }
}

/// <summary>
/// Provides extension methods for the Chat widget that enable a fluent API for
/// configuring chat appearance and behavior. These methods allow you to easily
/// customize the chat interface for optimal user experience and functionality.
/// </summary>
public static class ChatExtensions
{
    /// <summary>
    /// Sets the placeholder text for the chat input field.
    /// This method allows you to customize the guidance text displayed in
    /// the message input field, improving user experience by providing
    /// clear instructions about expected input.
    /// </summary>
    /// <param name="chat">The Chat widget to configure.</param>
    /// <param name="placeholder">The placeholder text to display in the input field.</param>
    /// <returns>The Chat instance for method chaining.</returns>
    public static Chat Placeholder(this Chat chat, string placeholder)
    {
        chat.Placeholder = placeholder;
        return chat;
    }
}

/// <summary>
/// Represents a single message within a chat conversation, containing the
/// message content and sender information. This record provides the fundamental
/// building block for chat interfaces, enabling structured conversation display
/// and management.
/// 
/// ChatMessage objects can contain various types of content including text,
/// rich media, interactive elements, and custom widgets, making them versatile
/// for different chat use cases and content requirements.
/// </summary>
public record ChatMessage : WidgetBase<ChatMessage>
{
    /// <summary>
    /// Initializes a new instance of the ChatMessage class with the specified
    /// sender and content. The message will be displayed in the chat interface
    /// with appropriate styling based on the sender type.
    /// </summary>
    /// <param name="sender">The ChatSender type indicating whether this message
    /// is from the user or an assistant, determining the visual styling and
    /// layout positioning of the message.</param>
    /// <param name="content">The content to display in the message. This can be
    /// text, rich content, interactive elements, or any other widget that
    /// should be rendered as part of the message.</param>
    public ChatMessage(ChatSender sender, object content) : base(content)
    {
        Sender = sender;
    }

    /// <summary>
    /// Gets or sets the sender type for this chat message.
    /// This property determines how the message is displayed in the chat
    /// interface, with different styling and positioning for user messages
    /// versus assistant messages.
    /// 
    /// The sender type affects the visual appearance, layout positioning,
    /// and styling of the message to provide clear visual distinction
    /// between different participants in the conversation.
    /// </summary>
    [Prop] public ChatSender Sender { get; set; }
}

/// <summary>
/// Represents a loading state within a chat conversation, typically used
/// to indicate that the system is processing a request or generating a
/// response. This widget provides visual feedback to users during
/// asynchronous operations.
/// 
/// ChatLoading widgets are commonly used to show processing states while
/// waiting for AI responses, API calls, or other time-consuming operations
/// that require user notification of ongoing activity.
/// </summary>
public record ChatLoading : WidgetBase<ChatLoading>
{
}

/// <summary>
/// Represents a status message within a chat conversation, typically used
/// to display system notifications, processing states, or informational
/// messages. This widget provides a way to communicate status information
/// to users without requiring a full response message.
/// 
/// ChatStatus widgets are ideal for showing loading indicators, system
/// notifications, error messages, or any other status information that
/// should be displayed within the chat flow.
/// </summary>
public record ChatStatus : WidgetBase<ChatStatus>
{
    /// <summary>
    /// Initializes a new instance of the ChatStatus class with the specified
    /// status text. The status message will be displayed in the chat interface
    /// to provide information or feedback to the user.
    /// </summary>
    /// <param name="text">The status text to display, typically describing
    /// the current state, processing information, or system notification
    /// that should be communicated to the user.</param>
    public ChatStatus(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Gets or sets the status text displayed in the chat status message.
    /// This property contains the informational content that communicates
    /// the current state, processing information, or system notification
    /// to the user.
    /// 
    /// The status text should be clear and concise, providing users with
    /// the information they need to understand what is happening or what
    /// they should expect next in the conversation flow.
    /// </summary>
    [Prop] public string Text { get; set; }
}