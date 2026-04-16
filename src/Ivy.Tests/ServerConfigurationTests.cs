using Microsoft.Extensions.Configuration;

namespace Ivy.Tests;

public class TestConnectionWithPresets : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath) => "";
    public string GetNamespace() => "Test";
    public string GetName() => "TestConnection";
    public string GetConnectionType() => "Test";
    public ConnectionEntity[] GetEntities() => [];
    public void RegisterServices(Server server) { }
    public Task<(bool ok, string? message)> TestConnection(IConfiguration config) => Task.FromResult((true, (string?)null));

    public Secret[] GetSecrets() =>
    [
        new Secret("Test:ApiKey", "preset-api-key-123"),
        new Secret("Test:Endpoint", "https://preset.example.com/"),
    ];
}

public class TestConnectionWithoutSecrets : IConnection
{
    public string GetContext(string connectionPath) => "";
    public string GetNamespace() => "Test";
    public string GetName() => "NoSecrets";
    public string GetConnectionType() => "Test";
    public ConnectionEntity[] GetEntities() => [];
    public void RegisterServices(Server server) { }
    public Task<(bool ok, string? message)> TestConnection(IConfiguration config) => Task.FromResult((true, (string?)null));
}

public class ServerConfigurationTests
{
    [Fact]
    public void AddConnectionsFromAssembly_LoadsSecretPresetsIntoConfiguration()
    {
        var server = new Server(new ServerArgs());

        server.AddConnectionsFromAssembly(typeof(ServerConfigurationTests).Assembly);

        Assert.Equal("preset-api-key-123", server.Configuration["Test:ApiKey"]);
        Assert.Equal("https://preset.example.com/", server.Configuration["Test:Endpoint"]);
    }

    [Fact]
    public void AddConnectionsFromAssembly_PresetsAreOverriddenByEnvironmentVariables()
    {
        var server = new Server(new ServerArgs());

        Environment.SetEnvironmentVariable("Test__ApiKey", "env-override-key");
        try
        {
            server.AddConnectionsFromAssembly(typeof(ServerConfigurationTests).Assembly);

            Assert.Equal("env-override-key", server.Configuration["Test:ApiKey"]);
            Assert.Equal("https://preset.example.com/", server.Configuration["Test:Endpoint"]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("Test__ApiKey", null);
        }
    }

    [Fact]
    public void AddConnectionsFromAssembly_WithNoPresets_DoesNotRebuildConfiguration()
    {
        var server = new Server(new ServerArgs());
        var originalConfig = server.Configuration;

        server.AddConnectionsFromAssembly(typeof(int).Assembly);

        Assert.Same(originalConfig, server.Configuration);
    }

    [Fact]
    public void AddConnectionsFromAssembly_CalledMultipleTimes_DoesNotCorruptConfiguration()
    {
        var server = new Server(new ServerArgs());

        server.AddConnectionsFromAssembly(typeof(ServerConfigurationTests).Assembly);
        server.AddConnectionsFromAssembly(typeof(ServerConfigurationTests).Assembly);

        Assert.Equal("preset-api-key-123", server.Configuration["Test:ApiKey"]);
        Assert.Equal("https://preset.example.com/", server.Configuration["Test:Endpoint"]);
    }

    [Fact]
    public void AddConnectionsFromAssembly_CalledMultipleTimes_DoesNotRegisterServicesTwice()
    {
        var server = new Server(new ServerArgs());

        server.AddConnectionsFromAssembly(typeof(ServerConfigurationTests).Assembly);
        var serviceCountAfterFirstCall = server.Services.Count;

        server.AddConnectionsFromAssembly(typeof(ServerConfigurationTests).Assembly);
        var serviceCountAfterSecondCall = server.Services.Count;

        Assert.Equal(serviceCountAfterFirstCall, serviceCountAfterSecondCall);
    }

    [Fact]
    public void TestConnectionValidation_PresetSecrets_AvailableInConfigurationAfterLoading()
    {
        var server = new Server(new ServerArgs());
        var connection = new TestConnectionWithPresets();
        var hasSecrets = (IHaveSecrets)connection;

        Assert.Null(server.Configuration["Test:ApiKey"]);

        server.AddConnectionsFromAssembly(typeof(ServerConfigurationTests).Assembly);

        var missing = hasSecrets.GetSecrets()
            .Where(s => !s.Optional && s.Preset == null && string.IsNullOrEmpty(server.Configuration[s.Key]))
            .Select(s => s.Key)
            .ToList();
        Assert.Empty(missing);

        foreach (var secret in hasSecrets.GetSecrets().Where(s => s.Preset != null))
        {
            Assert.False(string.IsNullOrEmpty(server.Configuration[secret.Key]),
                $"Preset secret '{secret.Key}' should have a value in Configuration");
        }
    }
}
