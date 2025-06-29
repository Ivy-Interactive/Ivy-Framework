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
    public Chat(ChatMessage[] messages, Action<Event<Chat, string>> onSendMessage) : base(messages.Cast<object>().ToArray())
    {
        OnSendMessage = onSendMessage;
        Width = Size.Full();
        Height = Size.Full();
    }

    [Event] public Action<Event<Chat, string>> OnSendMessage { get; set; }

    [Prop] public string Placeholder { get; set; } = "Type a message...";
}

public static class ChatExtensions
{
    public static Chat Placeholder(this Chat chat, string placeholder)
    {
        chat.Placeholder = placeholder;
        return chat;
    }
}

public record ChatMessage : WidgetBase<ChatMessage>
{
    public ChatMessage(ChatSender sender, object content) : base(content)
    {
        Sender = sender;
    }

    [Prop] public ChatSender Sender { get; set; }
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