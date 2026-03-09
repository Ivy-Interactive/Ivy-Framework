// Resharper disable once CheckNamespace
namespace Ivy;

public interface IClientSender
{
    public void Send(string method, object? data);
}

public interface IClientProvider
{
    public IClientSender Sender { get; set; }
}