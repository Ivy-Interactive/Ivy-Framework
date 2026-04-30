using Ivy.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: IvyPlugin(typeof(Ivy.Plugin.Slack.SlackPlugin))]

namespace Ivy.Plugin.Slack;

public class SlackPlugin : IIvyPlugin
{
    public PluginManifest Manifest { get; } = new()
    {
        Id = "Ivy.Plugin.Slack",
        Name = "Slack",
        ConfigSectionName = "Slack",
        Version = new Version(1, 0, 0),
    };

    public PluginConfigurationSchema ConfigurationSchema { get; } = new()
    {
        Fields =
        [
            new()
            {
                Key = "BotToken",
                Type = ConfigFieldType.Secret,
                IsRequired = true,
                Description = "Slack Bot User OAuth Token (starts with xoxb-)"
            },
            new()
            {
                Key = "DefaultChannel",
                Type = ConfigFieldType.String,
                IsRequired = false,
                Description = "Default channel ID or name for messages",
                DefaultValue = "general"
            },
            new()
            {
                Key = "MaxRetries",
                Type = ConfigFieldType.Integer,
                IsRequired = false,
                Description = "Maximum number of retry attempts for failed messages",
                DefaultValue = "3"
            }
        ]
    };

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void Configure(IPluginContext context)
    {
        var section = context.Configuration.GetSection("Plugins:Slack");

        // No manual default handling needed — host injects defaults
        var config = new SlackConfig
        {
            BotToken = section["BotToken"]!,
            DefaultChannel = section["DefaultChannel"]!,  // Guaranteed non-null due to default
            MaxRetries = int.Parse(section["MaxRetries"]!)  // Guaranteed valid integer
        };

        context.RegisterMessagingChannel(new SlackMessagingChannel(config));
    }
}
