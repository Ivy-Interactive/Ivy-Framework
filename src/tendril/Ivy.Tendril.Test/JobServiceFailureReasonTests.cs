using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceFailureReasonTests
{
    [Fact]
    public void ExtractFailureReason_EmptyOutput_ReturnsUnknownError()
    {
        var result = JobService.ExtractFailureReason([]);
        Assert.Equal("Unknown error (exit code non-zero)", result);
    }

    [Fact]
    public void ExtractFailureReason_StderrLines_ReturnsLastStderrContent()
    {
        var lines = new List<string>
        {
            "Starting process...",
            "[stderr] warning: something minor",
            "Processing...",
            "[stderr] error: connection refused",
            "[stderr] fatal: cannot continue"
        };

        var result = JobService.ExtractFailureReason(lines);
        Assert.Contains("fatal: cannot continue", result);
        Assert.Contains("error: connection refused", result);
    }

    [Fact]
    public void ExtractFailureReason_NoStderr_FallsBackToLastOutputLine()
    {
        var lines = new List<string>
        {
            "Step 1 done",
            "Step 2 done",
            "Build failed with 3 errors"
        };

        var result = JobService.ExtractFailureReason(lines);
        Assert.Equal("Build failed with 3 errors", result);
    }

    [Fact]
    public void ExtractFailureReason_LongLine_TruncatesTo200Chars()
    {
        var longLine = new string('x', 300);
        var lines = new List<string> { longLine };

        var result = JobService.ExtractFailureReason(lines);
        Assert.Equal(203, result.Length); // 200 + "..."
        Assert.EndsWith("...", result);
    }

    [Fact]
    public void ExtractFailureReason_OnlyEmptyLines_ReturnsUnknownError()
    {
        var lines = new List<string> { "", "  ", "" };

        var result = JobService.ExtractFailureReason(lines);
        Assert.Equal("Unknown error (exit code non-zero)", result);
    }

    [Fact]
    public void ExtractFailureReason_EmptyStderrLines_SkipsEmpty()
    {
        var lines = new List<string>
        {
            "[stderr] ",
            "[stderr] actual error message",
            "[stderr]  "
        };

        var result = JobService.ExtractFailureReason(lines);
        Assert.Equal("actual error message", result);
    }

    [Fact]
    public void ExtractFailureReason_MixedOutput_PrefersStderr()
    {
        var lines = new List<string>
        {
            "Some regular output",
            "[stderr] the real error",
            "More regular output after stderr"
        };

        var result = JobService.ExtractFailureReason(lines);
        Assert.Equal("the real error", result);
    }

    [Fact]
    public void CompleteJob_NonZeroExitCode_PopulatesStatusMessage()
    {
        var service = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));
        var id = service.StartJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id)!;
        job.OutputLines.Enqueue("[stderr] something went wrong");

        service.CompleteJob(id, 1);

        job = service.GetJob(id)!;
        Assert.Equal(JobStatus.Failed, job.Status);
        Assert.NotNull(job.StatusMessage);
        Assert.Contains("something went wrong", job.StatusMessage);
    }

    [Fact]
    public void CompleteJob_ZeroExitCode_NullStatusMessage()
    {
        var service = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));
        var id = service.StartJob("ExecutePlan", Path.GetTempPath());

        service.CompleteJob(id, 0);

        var job = service.GetJob(id)!;
        Assert.Equal(JobStatus.Completed, job.Status);
        Assert.Null(job.StatusMessage);
    }

    [Fact]
    public void ExtractFailureReason_AnsiCodes_StripsEscapeSequences()
    {
        var lines = new List<string>
        {
            "[stderr] \x1B[31merror: build failed\x1B[0m"
        };

        var result = JobService.ExtractFailureReason(lines);
        Assert.Equal("error: build failed", result);
    }

    [Fact]
    public void ExtractFailureReason_ControlCharacters_NormalizesWhitespace()
    {
        var lines = new List<string>
        {
            "[stderr] error:\tfailed to\t\tcompile\nwith errors"
        };

        var result = JobService.ExtractFailureReason(lines);
        Assert.Equal("error: failed to compile with errors", result);
        Assert.DoesNotContain("\t", result);
        Assert.DoesNotContain("\n", result);
    }

    [Fact]
    public void CompleteJob_WithExistingStatusMessage_PreservesApiSetMessage()
    {
        var service = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));
        var id = service.StartJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id)!;
        job.StatusMessage = "Execution failed (exit code: 1)";
        job.OutputLines.Enqueue("[stderr] some raw stderr output");

        service.CompleteJob(id, 1);

        job = service.GetJob(id)!;
        Assert.Equal(JobStatus.Failed, job.Status);
        Assert.Equal("Execution failed (exit code: 1)", job.StatusMessage);
    }
}