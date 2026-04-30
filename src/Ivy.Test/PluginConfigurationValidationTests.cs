using Ivy.Core.Apps;
using Ivy.Core.Plugins;
using Ivy.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

    private static IConfiguration BuildConfig(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    [Fact]
    public void ValidateConfiguration_RequiredFieldMissing_ReturnsError()
    {
        var loader = CreateLoader();
        var schema = new PluginConfigurationSchema
        {
            Fields =
            [
                new() { Key = "BotToken", Type = ConfigFieldType.Secret, IsRequired = true }
            ]
        };
        var config = BuildConfig(new Dictionary<string, string?>());

        var errors = loader.ValidatePluginConfiguration("Slack", schema, config);

        Assert.Single(errors);
        Assert.Contains("Required field 'BotToken' is missing", errors[0]);
    }

    [Fact]
    public void ValidateConfiguration_InvalidIntegerType_ReturnsError()
    {
        var loader = CreateLoader();
        var schema = new PluginConfigurationSchema
        {
            Fields =
            [
                new() { Key = "MaxRetries", Type = ConfigFieldType.Integer, IsRequired = false }
            ]
        };
        var config = BuildConfig(new Dictionary<string, string?>
        {
            ["Plugins:Test:MaxRetries"] = "not-a-number"
        });

        var errors = loader.ValidatePluginConfiguration("Test", schema, config);

        Assert.Single(errors);
        Assert.Contains("invalid type", errors[0]);
    }

    [Fact]
    public void ValidateConfiguration_InvalidBooleanType_ReturnsError()
    {
        var loader = CreateLoader();
        var schema = new PluginConfigurationSchema
        {
            Fields =
            [
                new() { Key = "Enabled", Type = ConfigFieldType.Boolean, IsRequired = false }
            ]
        };
        var config = BuildConfig(new Dictionary<string, string?>
        {
            ["Plugins:Test:Enabled"] = "not-a-bool"
        });

        var errors = loader.ValidatePluginConfiguration("Test", schema, config);

        Assert.Single(errors);
        Assert.Contains("invalid type", errors[0]);
    }

    [Fact]
    public void ValidateConfiguration_OptionalFieldMissing_NoError()
    {
        var loader = CreateLoader();
        var schema = new PluginConfigurationSchema
        {
            Fields =
            [
                new() { Key = "DefaultChannel", Type = ConfigFieldType.String, IsRequired = false }
            ]
        };
        var config = BuildConfig(new Dictionary<string, string?>());

        var errors = loader.ValidatePluginConfiguration("Slack", schema, config);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateConfiguration_AllFieldsValid_NoError()
    {
        var loader = CreateLoader();
        var schema = new PluginConfigurationSchema
        {
            Fields =
            [
                new() { Key = "BotToken", Type = ConfigFieldType.Secret, IsRequired = true },
                new() { Key = "DefaultChannel", Type = ConfigFieldType.String, IsRequired = false },
                new() { Key = "MaxRetries", Type = ConfigFieldType.Integer, IsRequired = false },
                new() { Key = "Enabled", Type = ConfigFieldType.Boolean, IsRequired = false }
            ]
        };
        var config = BuildConfig(new Dictionary<string, string?>
        {
            ["Plugins:Slack:BotToken"] = "xoxb-test-token",
            ["Plugins:Slack:DefaultChannel"] = "general",
            ["Plugins:Slack:MaxRetries"] = "3",
            ["Plugins:Slack:Enabled"] = "true"
        });

        var errors = loader.ValidatePluginConfiguration("Slack", schema, config);

        Assert.Empty(errors);
    }

    [Fact]
    public void Configure_InvalidConfiguration_SkipsPlugin()
    {
        var context = new TestPluginContext(new Dictionary<string, string?>());
        var configured = false;

        var plugin = new FakePlugin(
            schema: new PluginConfigurationSchema
            {
                Fields = [new() { Key = "Required", Type = ConfigFieldType.String, IsRequired = true }]
            },
            onConfigure: _ => configured = true);

        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-plugins-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var loader = new PluginLoader(tempDir, logger);

        // Use reflection to add the fake plugin to the internal list
        loader.AddTestPlugin(plugin, tempDir);
        loader.Configure(context);

        Assert.False(configured);
    }

    [Fact]
    public void Configure_ValidConfiguration_CallsPluginConfigure()
    {
        var context = new TestPluginContext(new Dictionary<string, string?>
        {
            ["Plugins:Fake:ApiKey"] = "test-key"
        });
        var configured = false;

        var plugin = new FakePlugin(
            schema: new PluginConfigurationSchema
            {
                Fields = [new() { Key = "ApiKey", Type = ConfigFieldType.String, IsRequired = true }]
            },
            onConfigure: _ => configured = true);

        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-plugins-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var loader = new PluginLoader(tempDir, logger);

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
    public void ApplyDefaults_OptionalFieldMissing_InjectsDefault()
    {
        var loader = CreateLoader();
        var schema = new PluginConfigurationSchema
        {
            Fields =
            [
                new() { Key = "MaxRetries", Type = ConfigFieldType.Integer, IsRequired = false, DefaultValue = "3" }
            ]
        };
        var config = BuildConfig(new Dictionary<string, string?>());

        var configWithDefaults = loader.ApplyConfigurationDefaults("Test", schema, config);
        var value = configWithDefaults.GetSection("Plugins:Test")["MaxRetries"];

        Assert.Equal("3", value);
    }

    [Fact]
    public void ApplyDefaults_OptionalFieldProvided_UsesProvidedValue()
    {
        var loader = CreateLoader();
        var schema = new PluginConfigurationSchema
        {
            Fields =
            [
                new() { Key = "MaxRetries", Type = ConfigFieldType.Integer, IsRequired = false, DefaultValue = "3" }
            ]
        };
        var config = BuildConfig(new Dictionary<string, string?>
        {
            ["Plugins:Test:MaxRetries"] = "5"
        });

        var configWithDefaults = loader.ApplyConfigurationDefaults("Test", schema, config);
        var value = configWithDefaults.GetSection("Plugins:Test")["MaxRetries"];

        Assert.Equal("5", value);
    }

    [Fact]
    public void ApplyDefaults_NoDefaultValue_DoesNotInject()
    {
        var loader = CreateLoader();
        var schema = new PluginConfigurationSchema
        {
            Fields =
            [
                new() { Key = "DefaultChannel", Type = ConfigFieldType.String, IsRequired = false }
            ]
        };
        var config = BuildConfig(new Dictionary<string, string?>());

        var configWithDefaults = loader.ApplyConfigurationDefaults("Test", schema, config);
        var value = configWithDefaults.GetSection("Plugins:Test")["DefaultChannel"];

        Assert.Null(value);
    }

    [Fact]
    public void ApplyDefaults_RequiredFieldWithDefault_StillValidatesPresence()
    {
        var loader = CreateLoader();
        var schema = new PluginConfigurationSchema
        {
            Fields =
            [
                new() { Key = "ApiKey", Type = ConfigFieldType.String, IsRequired = true, DefaultValue = "default-key" }
            ]
        };
        var config = BuildConfig(new Dictionary<string, string?>());

        var errors = loader.ValidatePluginConfiguration("Test", schema, config);

        Assert.Single(errors);
        Assert.Contains("Required field 'ApiKey' is missing", errors[0]);
    }

    [Fact]
    public void Configure_WithDefaults_PluginReceivesDefaultValues()
    {
        var context = new TestPluginContext(new Dictionary<string, string?>
        {
            ["Plugins:Fake:ApiKey"] = "test-key"
        });
        string? receivedMaxRetries = null;

        var plugin = new FakePlugin(
            schema: new PluginConfigurationSchema
            {
                Fields =
                [
                    new() { Key = "ApiKey", Type = ConfigFieldType.String, IsRequired = true },
                    new() { Key = "MaxRetries", Type = ConfigFieldType.Integer, IsRequired = false, DefaultValue = "3" }
                ]
            },
            onConfigure: ctx =>
            {
                var section = ctx.Configuration.GetSection("Plugins:Fake");
                receivedMaxRetries = section["MaxRetries"];
            });

        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-test-plugins-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var loader = new PluginLoader(tempDir, logger);

        loader.AddTestPlugin(plugin, tempDir);
        loader.Configure(context);

        Assert.Equal("3", receivedMaxRetries);
    }

    private class TestPluginContext : PluginContextBase
    {
        private readonly AppRepository _appRepository = new();
        private readonly WebApplicationBuilder _builder;
        private readonly IConfiguration _configuration;

        public TestPluginContext(Dictionary<string, string?> configValues)
        {
            _builder = WebApplication.CreateBuilder();
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();
        }

        public override IConfiguration Configuration => _configuration;
        protected override AppRepository AppRepository => _appRepository;
        protected override WebApplicationBuilder Builder => _builder;
    }

    private class FakePlugin : IIvyPlugin
    {
        private readonly Action<IPluginContext>? _onConfigure;

        public FakePlugin(PluginConfigurationSchema? schema, Action<IPluginContext>? onConfigure = null)
        {
            ConfigurationSchema = schema;
            _onConfigure = onConfigure;
        }

        public PluginManifest Manifest { get; } = new()
        {
            Id = "Ivy.Plugin.Fake",
            Name = "Fake",
            ConfigSectionName = "Fake",
            Version = new Version(1, 0, 0),
        };

        public PluginConfigurationSchema? ConfigurationSchema { get; }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration) { }

        public void Configure(IPluginContext context) => _onConfigure?.Invoke(context);
    }
}
