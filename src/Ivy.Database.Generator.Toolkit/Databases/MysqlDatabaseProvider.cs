using Microsoft.EntityFrameworkCore;

namespace Ivy.Database.Generator.Toolkit.Databases;

public class MysqlDatabaseProvider() : IDatabaseProvider
{
    public T GetDbContext<T>(string connectionString) where T : DbContext
    {
        var options = new DbContextOptionsBuilder<T>()
            .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 26)))
            .Options;
        return (T)Activator.CreateInstance(typeof(T), options)!;
    }
    
    public string? GetDefaultConnectionString(string? projectDirectory)
    {
        return null;
    }
}