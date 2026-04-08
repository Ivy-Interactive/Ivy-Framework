using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceLogCostTests
{
    [Fact]
    public void LogCostToCsv_CreatesFileWithHeaders()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            JobService.LogCostToCsv(tempDir, "ExecutePlan", 150000, 0.4500);

            var csvPath = Path.Combine(tempDir, "costs.csv");
            Assert.True(File.Exists(csvPath));

            var lines = File.ReadAllLines(csvPath);
            Assert.Equal("Promptware,Tokens,Cost", lines[0]);
            Assert.Equal("ExecutePlan,150000,0.4500", lines[1]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void LogCostToCsv_AppendsToExistingFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            JobService.LogCostToCsv(tempDir, "ExecutePlan", 150000, 0.4500);
            JobService.LogCostToCsv(tempDir, "MakePlan", 25000, 0.0750);

            var csvPath = Path.Combine(tempDir, "costs.csv");
            var lines = File.ReadAllLines(csvPath);
            Assert.Equal(3, lines.Length);
            Assert.Equal("Promptware,Tokens,Cost", lines[0]);
            Assert.Equal("ExecutePlan,150000,0.4500", lines[1]);
            Assert.Equal("MakePlan,25000,0.0750", lines[2]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void LogCostToCsv_SkipsNonexistentDirectory()
    {
        // Should not throw
        JobService.LogCostToCsv("/nonexistent/path/123", "Test", 100, 0.01);
    }

    [Fact]
    public void LogCostToCsv_FormatsCorrectly()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            JobService.LogCostToCsv(tempDir, "MakePr", 99999, 1.23456789);

            var csvPath = Path.Combine(tempDir, "costs.csv");
            var lines = File.ReadAllLines(csvPath);
            // Cost should be formatted to 4 decimal places
            Assert.Equal("MakePr,99999,1.2346", lines[1]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}