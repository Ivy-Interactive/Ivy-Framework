namespace Ivy.Core.Server;

/// <summary>
/// Configuration for WebMCP support, enabled with <c>server.UseWebMcp()</c>.
/// </summary>
public class WebMcpOptions
{
    /// <summary>
    /// Origin trial token for the Chrome WebMCP trial. When set it is emitted as
    /// <c>&lt;meta http-equiv="origin-trial"&gt;</c>, which is how a production origin opts in.
    /// Leave null when running on localhost or behind a browser flag.
    /// </summary>
    public string? OriginTrialToken { get; set; }
}
