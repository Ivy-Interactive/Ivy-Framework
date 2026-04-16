using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Ivy.Core.Server;

public static class ServerUtils
{
    public static IConfiguration GetConfiguration(
        IEnumerable<KeyValuePair<string, string?>>? initialSources = null,
        Action<IConfigurationBuilder>? configure = null)
    {
        var builder = new ConfigurationBuilder();

        if (initialSources != null)
        {
            builder.AddInMemoryCollection(initialSources);
        }

        builder.AddEnvironmentVariables()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        if (Assembly.GetEntryAssembly() is { } entryAssembly)
        {
            builder.AddUserSecrets(entryAssembly);
        }

        configure?.Invoke(builder);

        return builder.Build();
    }

    public static ServerArgs GetArgs()
    {
        var parser = new ArgsParser();
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
        var parsedArgs = parser.Parse(args);
        var serverArgs = new ServerArgs()
        {
            Port = parser.GetValue(parsedArgs, "port", ServerArgs.DefaultPort),
            Verbose = parser.GetValue(parsedArgs, "verbose", false),
            IKillForThisPort = parser.GetValue(parsedArgs, "i-kill-for-this-port", false),
            Browse = parser.GetValue(parsedArgs, "browse", false),
            Args = parser.GetValue<string?>(parsedArgs, "args", null),
            DefaultAppId = parser.GetValue<string?>(parsedArgs, "app", null),
            Silent = parser.GetValue(parsedArgs, "silent", false),
            Describe = parser.GetValue(parsedArgs, "describe", false),
            DescribeConnection = parser.GetValue<string?>(parsedArgs, "describe-connection", null),
            TestConnection = parser.GetValue<string?>(parsedArgs, "test-connection", null),
            EnableDevTools = parser.GetValue(parsedArgs, "enable-dev-tools", false),
            Host = parser.GetValue<string?>(parsedArgs, "host", null),
            BasePath = parser.GetValue<string?>(parsedArgs, "path-base", null),
        };
        serverArgs = serverArgs with { FindAvailablePort = parser.GetValue(parsedArgs, "find-available-port", false) };
        if (serverArgs.IsCliCommand)
        {
            serverArgs = serverArgs with { Silent = true };
        }
        return serverArgs;
    }
}