using System.Net.Http.Headers;
using Ivy;

namespace Ivy.Auth.Examples.Shared;

public class GitHubOAuthTestView : ViewBase
{
    private readonly OAuthProviderToken _token;
    private readonly string _appName;

    public GitHubOAuthTestView(OAuthProviderToken token, string appName = "IvyAuthExample")
    {
        _token = token;
        _appName = appName;
    }

    public override object? Build()
    {
        var apiResponse = UseState<string?>();

        return Layout.Vertical(
            Text.H4("GitHub OAuth Test"),
            Layout.Horizontal(
                new Button("Get GitHub User", async () =>
                {
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _token.AuthToken.AccessToken);
                    httpClient.DefaultRequestHeaders.UserAgent.Add(
                        new ProductInfoHeaderValue(_appName, "1.0"));

                    try
                    {
                        var response = await httpClient.GetStringAsync("https://api.github.com/user");
                        apiResponse.Set(response);
                    }
                    catch (Exception ex)
                    {
                        apiResponse.Set($"{{\"error\": \"{ex.Message}\"}}");
                    }
                }, variant: ButtonVariant.Primary),
                new Button("Fetch My Repositories", async () =>
                {
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _token.AuthToken.AccessToken);
                    httpClient.DefaultRequestHeaders.UserAgent.Add(
                        new ProductInfoHeaderValue(_appName, "1.0"));

                    try
                    {
                        var response = await httpClient.GetStringAsync("https://api.github.com/user/repos?per_page=10");
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
