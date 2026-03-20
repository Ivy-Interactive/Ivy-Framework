using Microsoft.Extensions.Configuration;
using Ivy;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App("Concepts/Connections", "Demonstrates the IConnection and IHaveSecrets interfaces for external integrations.")]
public class ConnectionsApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical(
            Text.H1("Connection Showcase"),
            Text.P("This sample demonstrates how to implement the `IConnection` and `IHaveSecrets` interfaces to integrate external services into an Ivy application."),

            new Card()
                .Title("Stripe Connection (Mock)")
                .Content(
                    Layout.Vertical(
                        Text.P("The `StripeConnection` class implements `IConnection` to register services and `IHaveSecrets` to declare required API keys."),
                        new CodeBlock(@"
public class StripeConnection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath) => string.Empty;
    public string GetNamespace() => typeof(StripeConnection).Namespace!;
    public string GetName() => ""Stripe"";
    public string GetConnectionType() => ""PaymentAPI"";
    
    public ConnectionEntity[] GetEntities() => [
        new(""Customer"", ""Customers""),
        new(""Payment"", ""Payments"")
    ];
    
    public void RegisterServices(Server server)
    {
        // Register services on the server
        // server.Services.AddSingleton<IStripeClient, StripeClient>();
    }

    public Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        var apiKey = config[""Stripe:ApiKey""];
        if (string.IsNullOrEmpty(apiKey)) 
            return Task.FromResult((false, ""API Key is missing""));
            
        return Task.FromResult((true, (string?)null));
    }

    public Secret[] GetSecrets() => [
        new Secret(""Stripe:ApiKey""),
        new Secret(""Stripe:PublishableKey"")
    ];
}")
                        .Language(Languages.Csharp)
                    ).Gap(4)
                )
        ).Gap(8);
    }
}

public class StripeConnection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath) => string.Empty;
    public string GetNamespace() => typeof(StripeConnection).Namespace!;
    public string GetName() => "Stripe";
    public string GetConnectionType() => "PaymentAPI";
    public ConnectionEntity[] GetEntities() => [new("Customer", "Customers"), new("Payment", "Payments")];

    public void RegisterServices(Server server)
    {
        // This is where you would register services
    }

    public Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        return Task.FromResult((true, (string?)null));
    }

    public Secret[] GetSecrets() => [new Secret("Stripe:ApiKey"), new Secret("Stripe:PublishableKey")];
}
