namespace Ivy.Plugin.Slack;

public sealed class SlackConfig
{
    public required string BotToken { get; set; }
    public string? DefaultChannel { get; set; }
    public int MaxRetries { get; set; } = 3;
}
