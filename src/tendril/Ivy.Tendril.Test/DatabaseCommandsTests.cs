using Ivy.Tendril.Database;

namespace Ivy.Tendril.Test;

public class DatabaseCommandsTests : IDisposable
{
    private readonly string _dbPath;

    public DatabaseCommandsTests()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"tendril-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        _dbPath = Path.Combine(tempDir, "tendril.db");
    }

    public void Dispose()
    {
        var dir = Path.GetDirectoryName(_dbPath)!;
        if (Directory.Exists(dir))
            try
            {
                Directory.Delete(dir, true);
            }
            catch
            {
                /* best effort cleanup */
            }
    }

    [Fact]
    public void DbVersion_ShowsCurrentAndLatestVersion()
    {
        // Initialize DB with migrations first
        DatabaseCommands.DbMigrate(_dbPath);

        var output = CaptureConsoleOutput(() =>
        {
            var result = DatabaseCommands.DbVersion(_dbPath);
            Assert.Equal(0, result);
        });

        Assert.Contains("Database version:", output);
        Assert.Contains("Latest version:", output);
        Assert.Contains("Status:", output);
        Assert.Contains("Up to date", output);
    }

    [Fact]
    public void DbMigrate_AppliesPendingMigrations()
    {
        var output = CaptureConsoleOutput(() =>
        {
            var result = DatabaseCommands.DbMigrate(_dbPath);
            Assert.Equal(0, result);
        });

        // Verify migrations were applied by checking version
        var versionOutput = CaptureConsoleOutput(() => DatabaseCommands.DbVersion(_dbPath));
        Assert.Contains("Up to date", versionOutput);
    }

    [Fact]
    public void DbMigrate_WhenUpToDate_ReportsUpToDate()
    {
        // Run migrations first
        DatabaseCommands.DbMigrate(_dbPath);

        // Run again — should report up to date
        var output = CaptureConsoleOutput(() =>
        {
            var result = DatabaseCommands.DbMigrate(_dbPath);
            Assert.Equal(0, result);
        });

        Assert.Contains("up to date", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DbReset_DropsTablesAndReappliesMigrations()
    {
        // Apply migrations to create tables
        DatabaseCommands.DbMigrate(_dbPath);

        // Reset with --force to skip confirmation
        var output = CaptureConsoleOutput(() =>
        {
            var result = DatabaseCommands.DbReset(_dbPath, ["db-reset", "--force"]);
            Assert.Equal(0, result);
        });

        Assert.Contains("Resetting database", output);
        Assert.Contains("Dropping existing tables", output);

        // Verify migrations were re-applied
        var versionOutput = CaptureConsoleOutput(() => DatabaseCommands.DbVersion(_dbPath));
        Assert.Contains("Up to date", versionOutput);
    }

    [Fact]
    public void DbReset_WithoutForce_AbortsOnNonYResponse()
    {
        DatabaseCommands.DbMigrate(_dbPath);

        var output = CaptureConsoleOutputWithInput("n\n", () =>
        {
            var result = DatabaseCommands.DbReset(_dbPath, ["db-reset"]);
            Assert.Equal(1, result);
        });

        Assert.Contains("Aborted", output);
    }

    [Fact]
    public void Handle_UnknownCommand_ReturnsNegativeOne()
    {
        var result = DatabaseCommands.Handle(["unknown-command"]);
        Assert.Equal(-1, result);
    }

    [Fact]
    public void Handle_EmptyArgs_ReturnsNegativeOne()
    {
        var result = DatabaseCommands.Handle([]);
        Assert.Equal(-1, result);
    }

    [Fact]
    public void Handle_DbVersion_ReturnsZero()
    {
        DatabaseCommands.DbMigrate(_dbPath);
        CaptureConsoleOutput(() => { Assert.Equal(0, DatabaseCommands.DbVersion(_dbPath)); });
    }

    [Fact]
    public void Handle_DbMigrate_ReturnsZero()
    {
        CaptureConsoleOutput(() => { Assert.Equal(0, DatabaseCommands.DbMigrate(_dbPath)); });
    }

    [Fact]
    public void Handle_DbReset_WithForce_ReturnsZero()
    {
        DatabaseCommands.DbMigrate(_dbPath);
        CaptureConsoleOutput(() => { Assert.Equal(0, DatabaseCommands.DbReset(_dbPath, ["db-reset", "--force"])); });
    }

    private static string CaptureConsoleOutput(Action action)
    {
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            action();
            return writer.ToString();
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    private static string CaptureConsoleOutputWithInput(string input, Action action)
    {
        var originalOut = Console.Out;
        var originalIn = Console.In;
        using var writer = new StringWriter();
        using var reader = new StringReader(input);
        Console.SetOut(writer);
        Console.SetIn(reader);
        try
        {
            action();
            return writer.ToString();
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
        }
    }
}