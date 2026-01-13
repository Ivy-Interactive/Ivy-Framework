using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Database.Generator.Toolkit.Databases;

public class SqliteDatabaseProvider() : IDatabaseProvider
{
    public T GetDbContext<T>(string connectionString) where T : DbContext
    {
        EnsureDatabase(connectionString);
        var options = new DbContextOptionsBuilder<T>()
            .UseSqlite(connectionString)
            .Options;
        return (T)Activator.CreateInstance(typeof(T), options)!;
    }

    private void EnsureDatabase(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
    }
    
    public string? GetDefaultConnectionString(string? projectDirectory)
    {
        var path = "db.sqlite";
        if (projectDirectory != null)
        {
            path = Path.Combine(projectDirectory, path);
        }
        return $"Data Source={path}";
    }
}