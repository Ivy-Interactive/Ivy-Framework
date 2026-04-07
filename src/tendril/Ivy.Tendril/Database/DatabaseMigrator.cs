using System.Reflection;
using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Database;

public class DatabaseMigrator
{
    private readonly SqliteConnection _connection;
    private readonly List<IMigration> _migrations;

    public DatabaseMigrator(SqliteConnection connection)
    {
        _connection = connection;
        _migrations = LoadMigrations();
    }

    internal DatabaseMigrator(SqliteConnection connection, List<IMigration> migrations)
    {
        _connection = connection;
        _migrations = migrations;
        ValidateMigrationSequence(_migrations);
    }

    public int GetCurrentVersion()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "PRAGMA user_version;";
        var result = cmd.ExecuteScalar();
        return Convert.ToInt32(result);
    }

    public int GetLatestVersion()
    {
        return _migrations.Count > 0 ? _migrations.Max(m => m.Version) : 0;
    }

    public void ApplyMigrations()
    {
        var currentVersion = GetCurrentVersion();
        var latestVersion = GetLatestVersion();

        if (currentVersion == latestVersion)
        {
            Console.WriteLine($"Database is up to date (version {currentVersion})");
            return;
        }

        if (currentVersion > latestVersion)
            throw new InvalidOperationException(
                $"Database version ({currentVersion}) is newer than application version ({latestVersion}). " +
                "Please update the application.");

        Console.WriteLine($"Migrating database from version {currentVersion} to {latestVersion}...");

        var pendingMigrations = _migrations
            .Where(m => m.Version > currentVersion)
            .OrderBy(m => m.Version)
            .ToList();

        using var transaction = _connection.BeginTransaction();

        try
        {
            foreach (var migration in pendingMigrations)
            {
                Console.WriteLine($"  Applying migration {migration.Version}: {migration.Description}");
                migration.Apply(_connection);

                var newVersion = GetCurrentVersion();
                if (newVersion != migration.Version)
                    throw new InvalidOperationException(
                        $"Migration {migration.Version} did not set PRAGMA user_version correctly. " +
                        $"Expected {migration.Version}, got {newVersion}");
            }

            transaction.Commit();
            Console.WriteLine($"Migration complete. Database is now at version {latestVersion}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Migration failed: {ex.Message}");
            transaction.Rollback();
            throw;
        }
    }

    private static List<IMigration> LoadMigrations()
    {
        var migrationType = typeof(IMigration);
        var migrations = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => migrationType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(t => (IMigration)Activator.CreateInstance(t)!)
            .OrderBy(m => m.Version)
            .ToList();

        ValidateMigrationSequence(migrations);

        return migrations;
    }

    private static void ValidateMigrationSequence(List<IMigration> migrations)
    {
        if (migrations.Count == 0) return;

        var duplicates = migrations.GroupBy(m => m.Version)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
            throw new InvalidOperationException(
                $"Duplicate migration versions found: {string.Join(", ", duplicates)}");

        for (var i = 0; i < migrations.Count; i++)
        {
            var expected = i + 1;
            var actual = migrations[i].Version;

            if (actual != expected)
                throw new InvalidOperationException(
                    $"Migration sequence is invalid. Expected version {expected}, found {actual}. " +
                    "Migrations must be numbered sequentially starting from 1.");
        }
    }
}