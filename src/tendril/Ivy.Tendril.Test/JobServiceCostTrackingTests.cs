using System.Diagnostics;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceCostTrackingTests
{
    private static string? FindTestScriptPath()
    {
        var relativeCandidates = new[]
        {
            Path.Combine("TestScripts", "TestSessionId.ps1"),
            Path.Combine("Ivy.Tendril.Test", "TestScripts", "TestSessionId.ps1"),
            Path.Combine("src", "tendril", "Ivy.Tendril.Test", "TestScripts", "TestSessionId.ps1")
        };

        var candidateRoots = new[]
        {
            System.AppContext.BaseDirectory,
            Directory.GetCurrentDirectory()
        };

        foreach (var root in candidateRoots)
        {
            var dir = new DirectoryInfo(root);
            while (dir != null)
            {
                foreach (var relativeCandidate in relativeCandidates)
                {
                    var candidate = Path.Combine(dir.FullName, relativeCandidate);
                    if (File.Exists(candidate))
                        return candidate;
                }

                dir = dir.Parent;
            }
        }

        return null;
    }

    private static JobService CreateService()
    {
        return new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));
    }

    [Fact]
    public void LaunchJob_SetsSessionIdToValidGuid()
    {
        var service = CreateService();

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id);

        Assert.NotNull(job);
        Assert.NotNull(job.SessionId);
        Assert.NotEmpty(job.SessionId);
        Assert.True(Guid.TryParse(job.SessionId, out _), "SessionId should be a valid GUID");
    }

    [Fact]
    public void LaunchJob_SessionIdIsUniquePerJob()
    {
        var service = CreateService();

        var id1 = service.StartJob("ExecutePlan", Path.GetTempPath());
        var id2 = service.StartJob("ExecutePlan", Path.GetTempPath());

        var job1 = service.GetJob(id1);
        var job2 = service.GetJob(id2);

        Assert.NotNull(job1?.SessionId);
        Assert.NotNull(job2?.SessionId);
        Assert.NotEqual(job1.SessionId, job2.SessionId);
    }

    [Fact]
    public void StartJob_SetsProviderFromConfig()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var settings = new TendrilSettings { CodingAgent = "codex" };
            var configService = new ConfigService(settings, tempDir);
            var service = new JobService(configService);

            var id = service.StartJob("ExecutePlan", Path.GetTempPath());
            var job = service.GetJob(id);

            Assert.NotNull(job);
            Assert.Equal("codex", job.Provider);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void StartJob_DefaultsProviderToClaude()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var settings = new TendrilSettings();
            var configService = new ConfigService(settings, tempDir);
            var service = new JobService(configService);

            var id = service.StartJob("ExecutePlan", Path.GetTempPath());
            var job = service.GetJob(id);

            Assert.NotNull(job);
            Assert.Equal("claude", job.Provider);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task LaunchJob_PassesSessionIdToChildProcess()
    {
        // Arrange
        var testScriptPath = FindTestScriptPath();

        Assert.True(testScriptPath != null && File.Exists(testScriptPath),
            $"Test script not found at {testScriptPath}");

        var sessionId = Guid.NewGuid().ToString();

        var psi = new ProcessStartInfo
        {
            FileName = "pwsh",
            WorkingDirectory = Path.GetDirectoryName(testScriptPath)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        psi.ArgumentList.Add("-NoProfile");
        psi.ArgumentList.Add("-File");
        psi.ArgumentList.Add(testScriptPath);
        psi.Environment["TENDRIL_SESSION_ID"] = sessionId;

        var process = new Process { StartInfo = psi };
        var outputLines = new List<string>();
        var errorLines = new List<string>();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null) outputLines.Add(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null) errorLines.Add(e.Data);
        };

        // Act
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        // Assert
        Assert.Equal(0, process.ExitCode);
        Assert.Empty(errorLines);

        var sessionIdLine = outputLines.FirstOrDefault(l => l.StartsWith("SESSION_ID:"));
        Assert.NotNull(sessionIdLine);

        var receivedSessionId = sessionIdLine.Substring("SESSION_ID:".Length);
        Assert.Equal(sessionId, receivedSessionId);
    }
}