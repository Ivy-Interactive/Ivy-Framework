namespace Ivy.Database.Generator.Toolkit.Databases;

public enum DatabaseProvider
{
    Sqlite,
    SqlServer,
    Postgres,
    MySql,
    MariaDb,
}

public static class DatabaseProviderFactory
{
    public static IDatabaseProvider Create(DatabaseProvider provider)
    {
        return provider switch
        {
            DatabaseProvider.SqlServer => new SqlServerDatabaseProvider(),
            DatabaseProvider.Postgres => new PostgresDatabaseProvider(),
            DatabaseProvider.MySql => new MysqlDatabaseProvider(),
            DatabaseProvider.MariaDb => new MariaDbDatabaseProvider(),
            DatabaseProvider.Sqlite => new SqliteDatabaseProvider(),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Unsupported database provider.")
        };
    }
}