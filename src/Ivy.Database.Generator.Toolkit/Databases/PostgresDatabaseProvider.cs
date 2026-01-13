using System.Net;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Ivy.Database.Generator.Toolkit.Databases;

public class PostgresDatabaseProvider() : IDatabaseProvider
{
    public T GetDbContext<T>(string connectionString) where T : DbContext
    {
        var processedConnectionString = ProcessPostgresConnectionString(connectionString);

        var options = new DbContextOptionsBuilder<T>()
            .UseNpgsql(processedConnectionString)
            .Options;
        return (T)Activator.CreateInstance(typeof(T), options)!;
    }

    public string? GetDefaultConnectionString(string? projectDirectory)
    {
        return null;
    }

    private static string ProcessPostgresConnectionString(string connectionString)
    {
        NpgsqlConnectionStringBuilder builder;
        try
        {
            builder = new NpgsqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException)
        {
            //In this case, we failed to parse an ADO.NET connection string, so we will try to
            //parse it as a URI-style string.
            //
            //Postgres connection string format: postgresql://user:password@host[:port]/dbname
            //Needs to be converted to: Host=host;Port=port;Database=dbname;Username=user;Password=password;SSL Mode=Require;Pooling=false;CommandTimeout=30;Keepalive=1
            //
            //If no port is specified, a default of 5432 will be used.
            //Pooling=false is important for Supabase/PgBouncer compatibility in some scenarios
            //todo: maybe use Pooling=true (the default) in some cases
            //Keepalive=1 can help with certain network environments but might increase chatter

            Uri uri;
            try
            {
                uri = new Uri(connectionString);
            }
            catch (UriFormatException)
            {
                throw new ArgumentException("Connection string is neither a valid ADO.NET connection string nor a valid PostgreSQL URI.", nameof(connectionString));
            }

            if (!uri.Scheme.Equals("postgresql", StringComparison.InvariantCultureIgnoreCase) &&
                !uri.Scheme.Equals("postgres",   StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException($"Unrecognized URI scheme '{uri.Scheme}'", nameof(connectionString));
            }

            var dbName = uri.AbsolutePath.Trim('/');
            dbName = WebUtility.UrlDecode(dbName).Trim();
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentException("Database name not found in connection URI", nameof(connectionString));
            }

            var userInfo = uri.UserInfo.Split(':');
            var username = WebUtility.UrlDecode(userInfo[0]).Trim();
            var password = userInfo.Length > 1
                ? WebUtility.UrlDecode(userInfo[1]).Trim()
                : "";

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username not found in connection URI", nameof(connectionString));
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password not found in connection URI", nameof(connectionString));
            }

            var port = uri.IsDefaultPort ? 5432 : uri.Port;

            var efConnectionString =
                $"Host={uri.Host};Port={port};Database={dbName};Username={username};Password={password};SSL Mode=Require;Pooling=false;CommandTimeout=30;Keepalive=1";

            builder = new NpgsqlConnectionStringBuilder(efConnectionString);
        }

        //Ensure required parameters are present for Npgsql/Supabase
        if (!builder.ContainsKey("SslMode") || builder.SslMode == Npgsql.SslMode.Disable || builder.SslMode == Npgsql.SslMode.Allow)
        {
            builder.SslMode = Npgsql.SslMode.Require;
        }

        return builder.ConnectionString;
    }
}