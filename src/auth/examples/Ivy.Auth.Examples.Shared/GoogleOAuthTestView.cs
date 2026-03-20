using System.Net.Http.Headers;
using Ivy;

namespace Ivy.Auth.Examples.Shared;

public class GoogleOAuthTestView : ViewBase
{
    private readonly IAuthTokenHandlerSession _session;

    public GoogleOAuthTestView(IAuthTokenHandlerSession session)
    {
        _session = session;
    }

    public override object? Build()
    {
        var apiResponse = UseState<string?>();

        return Layout.Vertical(
            Text.H4("Google OAuth Test"),
            new Button("Get Google Profile", async () =>
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _session.AuthToken?.AccessToken);

                try
                {
                    var response = await httpClient.GetStringAsync(
                        "https://www.googleapis.com/oauth2/v2/userinfo");
                    apiResponse.Set(response);
                }
                catch (Exception ex)
                {
                    apiResponse.Set($"{{\"error\": \"{ex.Message}\"}}");
                }
            }, variant: ButtonVariant.Primary),
            apiResponse.Value != null
                ? Text.Json(apiResponse.Value)
                : null
        ).Gap(10);
    }
}
