using Ivy.Core;

namespace Ivy.Client;

public class ToasterNotification
{
    public string? Title { get; set; }

    public string? Description { get; set; }
}

public static class ToasterExtensions
{
    public static void Toast(this IClientProvider client, string description, string? title = null)
    {
        client.Sender.Send("Toast", new ToasterNotification { Description = description, Title = title });
    }

    public static void Toast(this IClientProvider client, Exception ex)
    {
        var innerException = Utils.GetInnerMostException(ex);

        client.Sender.Send("Toast", new ToasterNotification { Description = innerException.Message, Title = "Failed" });
    }
}