using System.Diagnostics;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class GithubServiceTests
{
    [Fact]
    public void Should_Parse_Https_Url()
    {
        var repo = GithubService.ParseRepoConfigFromUrl("https://github.com/Ivy-Interactive/Ivy-Framework.git");
        Assert.NotNull(repo);
        Assert.Equal("Ivy-Interactive", repo.Owner);
        Assert.Equal("Ivy-Framework", repo.Name);
    }

    [Fact]
    public void Should_Parse_Https_Url_Without_Git_Suffix()
    {
        var repo = GithubService.ParseRepoConfigFromUrl("https://github.com/Ivy-Interactive/Ivy-Framework");
        Assert.NotNull(repo);
        Assert.Equal("Ivy-Interactive", repo.Owner);
        Assert.Equal("Ivy-Framework", repo.Name);
    }

    [Fact]
    public void Should_Parse_Ssh_Url()
    {
        var repo = GithubService.ParseRepoConfigFromUrl("git@github.com:Ivy-Interactive/Ivy-Agent.git");
        Assert.NotNull(repo);
        Assert.Equal("Ivy-Interactive", repo.Owner);
        Assert.Equal("Ivy-Agent", repo.Name);
    }

    [Fact]
    public void Should_Return_Null_For_Invalid_Url()
    {
        var repo = GithubService.ParseRepoConfigFromUrl("not-a-url");
        Assert.Null(repo);
    }

    [Fact]
    public void Should_Return_Empty_When_No_Projects()
    {
        var configService = new ConfigService(new TendrilSettings());
        var githubService = new GithubService(configService);
        var repos = githubService.GetRepos();
        Assert.Empty(repos);
    }

    [Fact]
    public void Should_Derive_Repos_From_Project_Paths()
    {
        var tempDir = CreateTempGitRepo("https://github.com/Test-Owner/Test-Repo.git");
        try
        {
            var settings = new TendrilSettings
            {
                Projects = [new ProjectConfig { Name = "TestProject", Repos = [new RepoRef { Path = tempDir }] }]
            };
            var configService = new ConfigService(settings);
            var githubService = new GithubService(configService);

            var repos = githubService.GetRepos();

            Assert.Single(repos);
            Assert.Equal("Test-Owner", repos[0].Owner);
            Assert.Equal("Test-Repo", repos[0].Name);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Should_Deduplicate_Repos_Across_Projects()
    {
        var tempDir = CreateTempGitRepo("https://github.com/Test-Owner/Test-Repo.git");
        try
        {
            var settings = new TendrilSettings
            {
                Projects =
                [
                    new ProjectConfig { Name = "Project1", Repos = [new RepoRef { Path = tempDir }] },
                    new ProjectConfig { Name = "Project2", Repos = [new RepoRef { Path = tempDir }] }
                ]
            };
            var configService = new ConfigService(settings);
            var githubService = new GithubService(configService);

            var repos = githubService.GetRepos();

            Assert.Single(repos);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static string CreateTempGitRepo(string remoteUrl)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ivy-github-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        RunGit(tempDir, "init");
        RunGit(tempDir, $"remote add origin {remoteUrl}");

        return tempDir;
    }

    private static void RunGit(string workingDir, string args)
    {
        var psi = new ProcessStartInfo("git", args)
        {
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = Process.Start(psi)!;
        process.WaitForExit();
    }
}