using System.Net.Http.Headers;
using System.Text.Json;

namespace Ivy.Auth.Examples.Shared;

/// <summary>Sample view: call Anthropic OAuth client_data with the brokered access token.</summary>
public class ClaudeOAuthTestView : ViewBase
{
    private const string DefaultProfileUrl = "https://api.anthropic.com/api/oauth/claude_cli/client_data";

    private readonly IAuthTokenHandlerSession _session;

    public ClaudeOAuthTestView(IAuthTokenHandlerSession session)
    {
        _session = session;
    }

    public override object? Build()
    {
        var rawJson = UseState<string?>();
        var error = UseState<string?>();

        return Layout.Vertical(
            Text.H4("Claude OAuth test"),
            new Button("Fetch OAuth profile (client_data)", async () =>
            {
                error.Set(null);
                rawJson.Set(null);
                var token = _session.AuthToken?.AccessToken;
                if (string.IsNullOrEmpty(token))
                {
                    error.Set("No access token in session.");
                    return;
                }

                try
                {
                    using var http = new HttpClient();
                    using var request = new HttpRequestMessage(HttpMethod.Get, DefaultProfileUrl);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await http.SendAsync(request);
                    var body = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        error.Set($"HTTP {(int)response.StatusCode}: {body}");
                        return;
                    }

                    rawJson.Set(JsonSerializer.Serialize(JsonDocument.Parse(body), new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (Exception ex)
                {
                    error.Set(ex.Message);
                }
            }, variant: ButtonVariant.Primary),
            error.Value != null ? Callout.Error(error.Value) : null,
            rawJson.Value != null ? Text.Monospaced(rawJson.Value) : null
        ).Gap(10);
    }
}
