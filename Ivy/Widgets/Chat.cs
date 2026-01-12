using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum ChatSender
{
    User,
    Assistant
}

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
        OnSend = onSend;
        OnCancel = onCancel;
    }

    internal Chat()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Event] public Func<Event<Chat, string>, ValueTask>? OnSend { get; set; }

    [Event] public Func<Event<Chat>, ValueTask>? OnCancel { get; set; }

    [Prop] public string Placeholder { get; set; } = "Type a message...";

    [Prop] public bool Streaming { get; set; }

    public Chat(ChatMessage[] messages, Action<Event<Chat, string>> onSend)
    : this(messages, e => { onSend(e); return ValueTask.CompletedTask; }, null)
    {
    }

    public Chat(
        ChatMessage[] messages,
        Action<Event<Chat, string>> onSend,
        Action<Event<Chat>>? onCancel
    ) : this(
        messages,
        e => { onSend(e); return ValueTask.CompletedTask; },
        onCancel != null ? e => { onCancel(e); return ValueTask.CompletedTask; }
    : null
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

    internal static Chat Streaming(this Chat chat, bool streaming)
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