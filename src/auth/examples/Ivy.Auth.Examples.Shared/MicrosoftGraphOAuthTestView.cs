using System.Net.Http.Headers;
using Ivy;

namespace Ivy.Auth.Examples.Shared;

public class MicrosoftGraphOAuthTestView : ViewBase
{
    private readonly IAuthTokenHandlerSession _session;

    public MicrosoftGraphOAuthTestView(IAuthTokenHandlerSession session)
    {
        _session = session;
    }

    public override object? Build()
    {
        var apiResponse = UseState<string?>();

        return Layout.Vertical(
            Text.H4("Microsoft Graph API Test"),
            Layout.Horizontal(
                new Baton("Get Profile", async () =>
                {
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _session.AuthToken?.AccessToken);

                    try
                    {
                        var response = await httpClient.GetStringAsync(
                            "https://graph.microsoft.com/v1.0/me");
                        apiResponse.Set(response);
                    }
                    catch (Exception ex)
                    {
                        apiResponse.Set($"{{\"error\": \"{ex.Message}\"}}");
                    }
                }, variant: BatonVariant.Primary),
                new Baton("List OneDrive Files", async () =>
                {
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _session.AuthToken?.AccessToken);

                    try
                    {
                        var response = await httpClient.GetStringAsync(
                            "https://graph.microsoft.com/v1.0/me/drive/root/children");
                        apiResponse.Set(response);
                    }
                    catch (Exception ex)
                    {
                        apiResponse.Set($"{{\"error\": \"{ex.Message}\"}}");
                    }
                }, variant: BatonVariant.Outline)
            ).Gap(10),
            apiResponse.Value != null
                ? Text.Json(apiResponse.Value)
                : null
        ).Gap(10);
    }
}
