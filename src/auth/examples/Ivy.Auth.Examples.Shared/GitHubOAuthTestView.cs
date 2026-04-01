using System.Net.Http.Headers;
using System.Text.Json;
using Ivy;

namespace Ivy.Auth.Examples.Shared;

public record GitHubRepo(
    string FullName,
    string? Description,
    int Stars,
    int Forks,
    string? Language,
    bool IsPrivate,
    string HtmlUrl
);

public record GitHubUser(
    string Login,
    string? Name,
    string? Bio,
    string? AvatarUrl,
    string HtmlUrl,
    int PublicRepos,
    int Followers,
    int Following,
    string? Location,
    string? Company
);

public class GitHubOAuthTestView : ViewBase
{
    private readonly IAuthTokenHandlerSession _session;
    private readonly string _appName;

    public GitHubOAuthTestView(IAuthTokenHandlerSession session, string appName = "IvyAuthExample")
    {
        _session = session;
        _appName = appName;
    }

    public override object? Build()
    {
        var userInfo = UseState<GitHubUser?>();
        var reposList = UseState<List<GitHubRepo>?>();

        return Layout.Vertical(
            Text.H4("GitHub OAuth Test"),
            Layout.Horizontal(
                new Button("Get GitHub User", async () =>
                {
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _session.AuthToken?.AccessToken);
                    httpClient.DefaultRequestHeaders.UserAgent.Add(
                        new ProductInfoHeaderValue(_appName, "1.0"));

                    try
                    {
                        var response = await httpClient.GetStringAsync("https://api.github.com/user");
                        using var doc = JsonDocument.Parse(response);
                        var user = new GitHubUser(
                            Login: doc.RootElement.GetProperty("login").GetString() ?? "Unknown",
                            Name: doc.RootElement.TryGetProperty("name", out var name) && name.ValueKind != JsonValueKind.Null
                                ? name.GetString()
                                : null,
                            Bio: doc.RootElement.TryGetProperty("bio", out var bio) && bio.ValueKind != JsonValueKind.Null
                                ? bio.GetString()
                                : null,
                            AvatarUrl: doc.RootElement.TryGetProperty("avatar_url", out var avatar) && avatar.ValueKind != JsonValueKind.Null
                                ? avatar.GetString()
                                : null,
                            HtmlUrl: doc.RootElement.GetProperty("html_url").GetString() ?? "",
                            PublicRepos: doc.RootElement.GetProperty("public_repos").GetInt32(),
                            Followers: doc.RootElement.GetProperty("followers").GetInt32(),
                            Following: doc.RootElement.GetProperty("following").GetInt32(),
                            Location: doc.RootElement.TryGetProperty("location", out var location) && location.ValueKind != JsonValueKind.Null
                                ? location.GetString()
                                : null,
                            Company: doc.RootElement.TryGetProperty("company", out var company) && company.ValueKind != JsonValueKind.Null
                                ? company.GetString()
                                : null
                        );
                        userInfo.Set(user);
                        reposList.Set((List<GitHubRepo>?)null);
                    }
                    catch (Exception)
                    {
                        userInfo.Set((GitHubUser?)null);
                        reposList.Set((List<GitHubRepo>?)null);
                    }
                }, variant: ButtonVariant.Primary),
                new Button("Fetch My Repositories", async () =>
                {
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _session.AuthToken?.AccessToken);
                    httpClient.DefaultRequestHeaders.UserAgent.Add(
                        new ProductInfoHeaderValue(_appName, "1.0"));

                    try
                    {
                        var response = await httpClient.GetStringAsync("https://api.github.com/user/repos?per_page=10");
                        using var doc = JsonDocument.Parse(response);
                        var repos = doc.RootElement.EnumerateArray()
                            .Select(repo => new GitHubRepo(
                                FullName: repo.GetProperty("full_name").GetString() ?? "Unknown",
                                Description: repo.TryGetProperty("description", out var desc) && desc.ValueKind != JsonValueKind.Null
                                    ? desc.GetString()
                                    : null,
                                Stars: repo.GetProperty("stargazers_count").GetInt32(),
                                Forks: repo.GetProperty("forks_count").GetInt32(),
                                Language: repo.TryGetProperty("language", out var lang) && lang.ValueKind != JsonValueKind.Null
                                    ? lang.GetString()
                                    : null,
                                IsPrivate: repo.GetProperty("private").GetBoolean(),
                                HtmlUrl: repo.GetProperty("html_url").GetString() ?? ""
                            ))
                            .ToList();
                        reposList.Set(repos);
                        userInfo.Set((GitHubUser?)null);
                    }
                    catch (Exception)
                    {
                        reposList.Set((List<GitHubRepo>?)null);
                        userInfo.Set((GitHubUser?)null);
                    }
                }, variant: ButtonVariant.Outline)
            ).Gap(10),
            userInfo.Value != null
                ? Layout.Horizontal(
                    userInfo.Value.AvatarUrl != null
                        ? new Image(userInfo.Value.AvatarUrl).Size(Size.Units(80))
                        : null,
                    Layout.Vertical(
                        Text.H5(userInfo.Value.Name ?? userInfo.Value.Login),
                        Text.Muted($"@{userInfo.Value.Login}"),
                        userInfo.Value.Bio != null ? Text.P(userInfo.Value.Bio) : null,
                        Layout.Horizontal(
                            userInfo.Value.Location != null ? Text.P($"📍 {userInfo.Value.Location}") : null,
                            userInfo.Value.Company != null ? Text.P($"🏢 {userInfo.Value.Company}") : null
                        ).Gap(10),
                        Layout.Horizontal(
                            Text.P($"📦 {userInfo.Value.PublicRepos} repos"),
                            Text.P($"👥 {userInfo.Value.Followers} followers"),
                            Text.P($"👤 {userInfo.Value.Following} following")
                        ).Gap(10),
                        new Button("View on GitHub").Url(userInfo.Value.HtmlUrl).Link().OpenInNewTab()
                    ).Gap(5)
                ).Gap(20).AlignContent(Align.Left)
                : null,
            reposList.Value != null
                ? Layout.Vertical(
                    reposList.Value.Select(repo =>
                        new Expandable(
                            header: repo.FullName,
                            content: Layout.Vertical(
                                repo.Description != null ? Text.P(repo.Description) : Text.Muted("No description"),
                                Layout.Horizontal(
                                    Text.P($"⭐ {repo.Stars}"),
                                    Text.P($"🍴 {repo.Forks}"),
                                    repo.Language != null ? Text.P($"💻 {repo.Language}") : null,
                                    Text.P(repo.IsPrivate ? "🔒 Private" : "🌐 Public")
                                ).Gap(10),
                                new Button("View on GitHub").Url(repo.HtmlUrl).Link().OpenInNewTab()
                            ).Gap(5)
                        )
                    ).ToArray()
                ).Gap(5)
                : null
        ).Gap(10);
    }
}
