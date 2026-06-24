using Ivy.Core.Apps;
using Ivy.Core.Plugins;
using Ivy.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace Ivy.Test.Plugins;

public class PluginConfigurationValidationTests
{
    private static PluginLoader CreateLoader()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-plugins-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        return new PluginLoader(tempDir, logger);
    }

    private static IIvyPluginConfig BuildConfig(Dictionary<string, string?> values) =>
        new TestPluginConfig(values);

    [Fact]
    public void ValidateConfiguration_RequiredFieldMissing_ReturnsError()
    {
        var loader = CreateLoader();
        var schema = new SchemaBuilder()
            .AddSecret("BotToken", isRequired: true)
            .Build();
        var config = BuildConfig([]);

        var errors = loader.ValidatePluginConfiguration(schema, config);

        Assert.Single(errors);
        Assert.Contains("Required field 'BotToken' is missing", errors[0]);
    }

    [Fact]
    public void ValidateConfiguration_InvalidIntegerType_ReturnsError()
    {
        var loader = CreateLoader();
        var schema = new SchemaBuilder()
            .AddInteger("MaxRetries")
            .Build();
        var config = BuildConfig(new Dictionary<string, string?>
        {
            ["MaxRetries"] = "not-a-number"
        });

        var errors = loader.ValidatePluginConfiguration(schema, config);

        Assert.Single(errors);
        Assert.Contains("invalid type", errors[0]);
    }

    [Fact]
    public void ValidateConfiguration_InvalidBooleanType_ReturnsError()
    {
        var loader = CreateLoader();
        var schema = new SchemaBuilder()
            .AddBoolean("Enabled")
            .Build();
        var config = BuildConfig(new Dictionary<string, string?>
        {
            ["Enabled"] = "not-a-bool"
        });

        var errors = loader.ValidatePluginConfiguration(schema, config);

        Assert.Single(errors);
        Assert.Contains("invalid type", errors[0]);
    }

    [Fact]
    public void ValidateConfiguration_OptionalFieldMissing_NoError()
    {
        var loader = CreateLoader();
        var schema = new SchemaBuilder()
            .AddString("DefaultChannel")
            .Build();
        var config = BuildConfig([]);

        var errors = loader.ValidatePluginConfiguration(schema, config);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateConfiguration_AllFieldsValid_NoError()
    {
        var loader = CreateLoader();
        var schema = new SchemaBuilder()
            .AddSecret("BotToken", isRequired: true)
            .AddString("DefaultChannel")
            .AddInteger("MaxRetries")
            .AddBoolean("Enabled")
            .Build();
        var config = BuildConfig(new Dictionary<string, string?>
        {
            ["BotToken"] = "xoxb-test-token",
            ["DefaultChannel"] = "general",
            ["MaxRetries"] = "3",
            ["Enabled"] = "true"
        });

        var errors = loader.ValidatePluginConfiguration(schema, config);

        Assert.Empty(errors);
    }

    [Fact]
    public void Configure_InvalidConfiguration_SkipsPlugin()
    {
        var context = new TestPluginContext();
        var configured = false;

        var plugin = new FakePlugin(
            schema: new SchemaBuilder()
                .AddString("Required", isRequired: true)
                .Build(),
            onConfigure: _ => configured = true);

        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-plugins-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var loader = new PluginLoader(tempDir, logger);
        loader.SetPluginConfigFactory(new TestPluginConfigFactory([]));

        loader.AddTestPlugin(plugin, tempDir);
        loader.Configure(context);

        Assert.False(configured);
    }

    [Fact]
    public void Configure_ValidConfiguration_CallsPluginConfigure()
    {
        var context = new TestPluginContext();
        var configured = false;

        var plugin = new FakePlugin(
            schema: new SchemaBuilder()
                .AddString("ApiKey", isRequired: true)
                .Build(),
            onConfigure: _ => configured = true);

        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-plugins-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var loader = new PluginLoader(tempDir, logger);
        loader.SetPluginConfigFactory(new TestPluginConfigFactory(new Dictionary<string, string?>
        {
            ["ApiKey"] = "test-key"
        }));

        loader.AddTestPlugin(plugin, tempDir);
        loader.Configure(context);

        Assert.True(configured);
    }

    [Fact]
    public void ValidateFieldType_ValidInteger_ReturnsTrue()
    {
        Assert.True(PluginLoader.ValidateFieldType("42", ConfigFieldType.Integer));
    }

    [Fact]
    public void ValidateFieldType_ValidBoolean_ReturnsTrue()
    {
        Assert.True(PluginLoader.ValidateFieldType("true", ConfigFieldType.Boolean));
        Assert.True(PluginLoader.ValidateFieldType("false", ConfigFieldType.Boolean));
    }

    [Fact]
    public void ValidateConfiguration_RequiredFieldWithDefault_StillValidatesPresence()
    {
        var loader = CreateLoader();
        var schema = new SchemaBuilder()
            .AddString("ApiKey", defaultValue: "default-key", isRequired: true)
            .Build();
        var config = BuildConfig([]);

        var errors = loader.ValidatePluginConfiguration(schema, config);

        Assert.Single(errors);
        Assert.Contains("Required field 'ApiKey' is missing", errors[0]);
    }

    [Fact]
    public void Configure_WithDefaults_PluginReceivesDefaultValues()
    {
        var context = new TestPluginContext();
        string? receivedMaxRetries = null;

        var plugin = new FakePlugin(
            schema: new SchemaBuilder()
                .AddString("ApiKey", isRequired: true)
                .AddInteger("MaxRetries", defaultValue: 3)
                .Build(),
            onConfigure: ctx =>
            {
                receivedMaxRetries = ctx.Config.GetValue("MaxRetries");
            });

        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-plugins-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var loader = new PluginLoader(tempDir, logger);
        loader.SetPluginConfigFactory(new TestPluginConfigFactory(new Dictionary<string, string?>
        {
            ["ApiKey"] = "test-key"
        }));

        loader.AddTestPlugin(plugin, tempDir);
        loader.Configure(context);

        Assert.Equal("3", receivedMaxRetries);
    }

    private class TestPluginContext()
        : PluginContextBase(
            new AppRepository(),
            new HashSet<string>(),
            WebApplication.CreateBuilder());

    private class TestPluginConfigFactory(Dictionary<string, string?> configValues) : IIvyPluginConfigFactory
    {
        public IIvyPluginConfig Create(string pluginId) => new TestPluginConfig(configValues);
    }

    private class TestPluginConfig(Dictionary<string, string?> configValues) : IIvyPluginConfig
    {
        public string? GetValue(string key) =>
            configValues.TryGetValue(key, out var value) ? value : null;

        public void SetValue(string key, string value) => configValues[key] = value;
        public void RemoveValue(string key) => configValues.Remove(key);
        public void Save() { }
    }

    private class FakePlugin : IIvyPlugin
    {
        private readonly Action<IIvyPluginContext>? _onConfigure;

        public FakePlugin(PluginConfigurationSchema? schema, Action<IIvyPluginContext>? onConfigure = null)
        {
            ConfigurationSchema = schema;
            _onConfigure = onConfigure;
        }

        public PluginManifest Manifest { get; } = new()
        {
            Id = "Ivy.Plugin.Fake",
            Title = "Fake",
            Version = new Version(1, 0, 0),
        };

        public PluginConfigurationSchema? ConfigurationSchema { get; }

        public void Configure(IIvyPluginContext context) => _onConfigure?.Invoke(context);
    }
}
