using System.Runtime.CompilerServices;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum ChatSender
{
    User,
    Assistant
}

/// <summary>
/// A complete chat interface component with message history and input.
/// </summary>
public record Chat : WidgetBase<Chat>
{
    [OverloadResolutionPriority(1)]
    public Chat(
        ChatMessage[] messages,
        Func<Event<Chat, string>, ValueTask> onSend,
        Func<Event<Chat>, ValueTask>? onCancel = null
    ) : base(messages.Cast<object>().ToArray())
    {
        Width = Size.Full();
        Height = Size.Full();
        OnSend = new(onSend);
        OnCancel = onCancel.ToEventHandler();
    }

    internal Chat()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Event] public EventHandler<Event<Chat, string>>? OnSend { get; set; }

    [Event] public EventHandler<Event<Chat>>? OnCancel { get; set; }

    [Prop] public string Placeholder { get; set; } = "Type a message...";

    [Prop] public bool Streaming { get; set; }

    public Chat(ChatMessage[] messages, Action<Event<Chat, string>> onSend)
    : this(messages, onSend.ToValueTask(), null)
    {
    }

    public Chat(
        ChatMessage[] messages,
        Action<Event<Chat, string>> onSend,
        Action<Event<Chat>>? onCancel
    ) : this(
        messages,
        onSend.ToValueTask(),
        onCancel?.ToValueTask()
    )
    {
    }
}

public static class ChatExtensions
{
    public static Chat Placeholder(this Chat chat, string placeholder)
    {
        chat.Placeholder = placeholder;
        return chat;
    }

    public static Chat Streaming(this Chat chat, bool streaming)
    {
        chat.Streaming = streaming;
        return chat;
    }
}

public record ChatMessage : WidgetBase<ChatMessage>
{
    public ChatMessage(ChatSender sender, object content) : base(content)
    {
        Sender = sender;
    }

    internal ChatMessage()
    {
    }

    [Prop] public ChatSender Sender { get; set; } = ChatSender.User;
}

public record ChatLoading : WidgetBase<ChatLoading>
{
}

public record ChatStatus : WidgetBase<ChatStatus>
{
    public ChatStatus(string text)
    {
        Text = text;
    }

    [Prop] public string Text { get; set; }
}