namespace Ivy.Core;

public interface IClientSender
{
    public void Send(string method, object? data);
}

public interface IClientProvider
{
    public IClientSender Sender { get; set; }
}