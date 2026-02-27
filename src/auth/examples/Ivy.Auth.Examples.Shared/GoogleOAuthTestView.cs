using System.Net.Http.Headers;
using Ivy;

namespace Ivy.Auth.Examples.Shared;

public class GoogleOAuthTestView : ViewBase
{
    private readonly OAuthProviderToken _token;

    public GoogleOAuthTestView(OAuthProviderToken token)
    {
        _token = token;
    }

    public override object? Build()
    {
        var apiResponse = UseState<string?>();

        return Layout.Vertical(
            Text.H4("Google OAuth Test"),
            Layout.Horizontal(
                new Button("Get Google Profile", async () =>
                {
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _token.AuthToken.AccessToken);

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
                new Button("List Google Drive Files", async () =>
                {
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _token.AuthToken.AccessToken);

                    try
                    {
                        var response = await httpClient.GetStringAsync(
                            "https://www.googleapis.com/drive/v3/files?pageSize=10");
                        apiResponse.Set(response);
                    }
                    catch (Exception ex)
                    {
                        apiResponse.Set($"{{\"error\": \"{ex.Message}\"}}");
                    }
                }, variant: ButtonVariant.Outline)
            ).Gap(10),
            apiResponse.Value != null
                ? Text.Json(apiResponse.Value)
                : null
        ).Gap(10);
    }
}
