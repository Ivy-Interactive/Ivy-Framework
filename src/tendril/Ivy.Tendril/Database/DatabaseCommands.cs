using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Database;

public static class DatabaseCommands
{
    /// <summary>
    ///     Handles database CLI commands. Returns exit code (0 = success, 1 = error),
    ///     or -1 if the args don't match a database command.
    /// </summary>
    public static int Handle(string[] args)
    {
        if (args.Length == 0) return -1;

        var tendrilHome = Environment.GetEnvironmentVariable("TENDRIL_HOME");
        if (string.IsNullOrEmpty(tendrilHome))
        {
            Console.Error.WriteLine("Error: TENDRIL_HOME environment variable is not set.");
            return 1;
        }

        var dbPath = Path.Combine(tendrilHome, "tendril.db");

        return args[0] switch
        {
            "db-version" => DbVersion(dbPath),
            "db-migrate" => DbMigrate(dbPath),
            "db-reset" => DbReset(dbPath, args),
            _ => -1
        };
    }

    internal static int DbVersion(string dbPath)
    {
        using var connection = OpenConnection(dbPath);
        var migrator = new DatabaseMigrator(connection);
        var current = migrator.GetCurrentVersion();
        var latest = migrator.GetLatestVersion();
        var status = current == latest ? "Up to date"
            : current > latest ? "Newer than application"
            : "Needs migration";

        Console.WriteLine($"Database version: {current}");
        Console.WriteLine($"Latest version:   {latest}");
        Console.WriteLine($"Status:           {status}");
        return 0;
    }

    internal static int DbMigrate(string dbPath)
    {
        using var connection = OpenConnection(dbPath);
        var migrator = new DatabaseMigrator(connection);
        migrator.ApplyMigrations();
        return 0;
    }

    internal static int DbReset(string dbPath, string[] args)
    {
        var force = args.Any(a => a == "--force");

        if (!force)
        {
            Console.Write("WARNING: This will delete all data in the database.\nAre you sure? (y/N): ");
            var response = Console.ReadLine();
            if (!string.Equals(response?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Aborted.");
                return 1;
            }
        }

        Console.WriteLine("Resetting database...");

        using var connection = OpenConnection(dbPath);

        // Get all table names
        var tables = new List<string>();
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name != 'sqlite_sequence';";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                tables.Add(reader.GetString(0));
        }

        // Drop all tables
        Console.WriteLine("  Dropping existing tables");
        foreach (var table in tables)
        {
            using var dropCmd = connection.CreateCommand();
            dropCmd.CommandText = $"DROP TABLE IF EXISTS \"{table}\";";
            dropCmd.ExecuteNonQuery();
        }

        // Reset version
        using (var versionCmd = connection.CreateCommand())
        {
            versionCmd.CommandText = "PRAGMA user_version = 0;";
            versionCmd.ExecuteNonQuery();
        }

        // Re-apply all migrations
        var migrator = new DatabaseMigrator(connection);
        migrator.ApplyMigrations();
        return 0;
    }

    private static SqliteConnection OpenConnection(string dbPath)
    {
        var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();
        using var pragmaCmd = connection.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
        pragmaCmd.ExecuteNonQuery();
        return connection;
    }
}