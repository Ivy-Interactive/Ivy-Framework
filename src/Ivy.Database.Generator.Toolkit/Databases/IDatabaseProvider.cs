using Microsoft.EntityFrameworkCore;

namespace Ivy.Database.Generator.Toolkit.Databases;

public interface IDatabaseProvider
{
    public T GetDbContext<T>(string connectionString) where T : DbContext;
    public string? GetDefaultConnectionString(string projectDirectory);
}