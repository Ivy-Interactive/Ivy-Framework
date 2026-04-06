using Ivy.Tendril.Database;
using Ivy.Tendril.Database.Migrations;
using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Test;

public class DatabaseMigratorTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public DatabaseMigratorTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        using var pragmaCmd = _connection.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
        pragmaCmd.ExecuteNonQuery();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private int GetUserVersion()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "PRAGMA user_version;";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private void SetUserVersion(int version)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = $"PRAGMA user_version = {version};";
        cmd.ExecuteNonQuery();
    }

    private bool TableExists(string tableName)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@name;";
        cmd.Parameters.AddWithValue("@name", tableName);
        return cmd.ExecuteScalar() != null;
    }

    [Fact]
    public void GetCurrentVersion_NewDatabase_ReturnsZero()
    {
        var migrator = new DatabaseMigrator(_connection, []);
        Assert.Equal(0, migrator.GetCurrentVersion());
    }

    [Fact]
    public void GetLatestVersion_WithMigrations_ReturnsHighestVersion()
    {
        var migrations = new List<IMigration>
        {
            new FakeMigration(1, "First"),
            new FakeMigration(2, "Second"),
            new FakeMigration(3, "Third")
        };
        var migrator = new DatabaseMigrator(_connection, migrations);
        Assert.Equal(3, migrator.GetLatestVersion());
    }

    [Fact]
    public void ApplyMigrations_NewDatabase_AppliesAllMigrations()
    {
        var applied = new List<int>();
        var migrations = new List<IMigration>
        {
            new FakeMigration(1, "First", applied),
            new FakeMigration(2, "Second", applied),
            new FakeMigration(3, "Third", applied)
        };

        var migrator = new DatabaseMigrator(_connection, migrations);
        migrator.ApplyMigrations();

        Assert.Equal([1, 2, 3], applied);
        Assert.Equal(3, GetUserVersion());
    }

    [Fact]
    public void ApplyMigrations_ExistingDatabase_UpgradesFromCurrentVersion()
    {
        // Simulate database already at version 2
        SetUserVersion(2);

        var applied = new List<int>();
        var migrations = new List<IMigration>
        {
            new FakeMigration(1, "First", applied),
            new FakeMigration(2, "Second", applied),
            new FakeMigration(3, "Third", applied)
        };

        var migrator = new DatabaseMigrator(_connection, migrations);
        migrator.ApplyMigrations();

        Assert.Equal([3], applied);
        Assert.Equal(3, GetUserVersion());
    }

    [Fact]
    public void ApplyMigrations_AlreadyUpToDate_DoesNothing()
    {
        SetUserVersion(2);

        var applied = new List<int>();
        var migrations = new List<IMigration>
        {
            new FakeMigration(1, "First", applied),
            new FakeMigration(2, "Second", applied)
        };

        var migrator = new DatabaseMigrator(_connection, migrations);
        migrator.ApplyMigrations();

        Assert.Empty(applied);
        Assert.Equal(2, GetUserVersion());
    }

    [Fact]
    public void ApplyMigrations_FailedMigration_RollsBack()
    {
        var migrations = new List<IMigration>
        {
            new FakeMigration(1, "First"),
            new FailingMigration(2, "Fails")
        };

        var migrator = new DatabaseMigrator(_connection, migrations);
        Assert.Throws<InvalidOperationException>(() => migrator.ApplyMigrations());

        // Version should still be 0 since transaction rolled back
        Assert.Equal(0, GetUserVersion());
    }

    [Fact]
    public void ApplyMigrations_InvalidSequence_ThrowsException()
    {
        var migrations = new List<IMigration>
        {
            new FakeMigration(1, "First"),
            new FakeMigration(3, "Third — skips 2")
        };

        Assert.Throws<InvalidOperationException>(() =>
            new DatabaseMigrator(_connection, migrations));
    }

    [Fact]
    public void ApplyMigrations_DuplicateVersions_ThrowsException()
    {
        var migrations = new List<IMigration>
        {
            new FakeMigration(1, "First"),
            new FakeMigration(1, "Duplicate")
        };

        Assert.Throws<InvalidOperationException>(() =>
            new DatabaseMigrator(_connection, migrations));
    }

    [Fact]
    public void ApplyMigrations_NewerDatabase_ThrowsException()
    {
        SetUserVersion(5);

        var migrations = new List<IMigration>
        {
            new FakeMigration(1, "First"),
            new FakeMigration(2, "Second"),
            new FakeMigration(3, "Third")
        };

        var migrator = new DatabaseMigrator(_connection, migrations);
        Assert.Throws<InvalidOperationException>(() => migrator.ApplyMigrations());
    }

    [Fact]
    public void Migration001_ExistingSchema_SetsVersionWithoutRecreating()
    {
        // Create the Plans table manually (simulating existing database)
        using var createCmd = _connection.CreateCommand();
        createCmd.CommandText = """
            CREATE TABLE Plans (
                Id INTEGER PRIMARY KEY,
                Title TEXT NOT NULL,
                FolderPath TEXT NOT NULL UNIQUE,
                FolderName TEXT NOT NULL,
                Project TEXT NOT NULL,
                Level TEXT NOT NULL,
                State TEXT NOT NULL,
                YamlRaw TEXT NOT NULL,
                RevisionCount INTEGER NOT NULL DEFAULT 1,
                LatestRevisionContent TEXT NOT NULL DEFAULT '',
                Created TEXT NOT NULL,
                Updated TEXT NOT NULL,
                InitialPrompt TEXT
            );
            """;
        createCmd.ExecuteNonQuery();

        // Insert a row to verify data survives
        using var insertCmd = _connection.CreateCommand();
        insertCmd.CommandText = """
            INSERT INTO Plans (Id, Title, Project, Level, State, FolderPath, FolderName,
                               YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated)
            VALUES (1, 'Test', 'Tendril', 'NiceToHave', 'Draft', '/test', 'test',
                    'yaml', 1, 'content', '2026-01-01', '2026-01-01')
            """;
        insertCmd.ExecuteNonQuery();

        // Apply migration 001
        var migration = new Migration_001_InitialSchema();
        migration.Apply(_connection);

        // Data should survive
        using var countCmd = _connection.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM Plans;";
        Assert.Equal(1, Convert.ToInt32(countCmd.ExecuteScalar()));

        Assert.Equal(1, GetUserVersion());
    }

    [Fact]
    public void Migration001_NewDatabase_CreatesSchemaAndSetsVersion()
    {
        var migration = new Migration_001_InitialSchema();
        migration.Apply(_connection);

        Assert.True(TableExists("Plans"));
        Assert.True(TableExists("Repos"));
        Assert.True(TableExists("Commits"));
        Assert.True(TableExists("PullRequests"));
        Assert.True(TableExists("Verifications"));
        Assert.True(TableExists("RelatedPlans"));
        Assert.True(TableExists("DependsOn"));
        Assert.True(TableExists("Costs"));
        Assert.True(TableExists("Recommendations"));
        Assert.True(TableExists("SyncMetadata"));
        Assert.Equal(1, GetUserVersion());
    }

    private class FakeMigration : IMigration
    {
        private readonly List<int>? _tracker;
        public int Version { get; }
        public string Description { get; }

        public FakeMigration(int version, string description, List<int>? tracker = null)
        {
            Version = version;
            Description = description;
            _tracker = tracker;
        }

        public void Apply(SqliteConnection connection)
        {
            _tracker?.Add(Version);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"PRAGMA user_version = {Version};";
            cmd.ExecuteNonQuery();
        }
    }

    private class FailingMigration : IMigration
    {
        public int Version { get; }
        public string Description { get; }

        public FailingMigration(int version, string description)
        {
            Version = version;
            Description = description;
        }

        public void Apply(SqliteConnection connection)
        {
            throw new InvalidOperationException("Intentional migration failure");
        }
    }
}
