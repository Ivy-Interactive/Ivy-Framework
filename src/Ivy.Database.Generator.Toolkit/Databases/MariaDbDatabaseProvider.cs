using Microsoft.EntityFrameworkCore;

namespace Ivy.Database.Generator.Toolkit.Databases;

public class MariaDbDatabaseProvider() : IDatabaseProvider
{
    public T GetDbContext<T>(string connectionString) where T : DbContext
    {
        var options = new DbContextOptionsBuilder<T>()
            .UseMySql(connectionString, new MariaDbServerVersion(new Version(10, 6, 4)))
            .Options;
        return (T)Activator.CreateInstance(typeof(T), options)!;
    }

    public string? GetDefaultConnectionString(string? projectDirectory)
    {
        return null;
    }
}