using System.Reflection;
using Ivy.Helpers;
using Microsoft.Extensions.Configuration;

namespace Ivy;

public static class ServerUtils
{
    public static IConfiguration GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        if (Assembly.GetEntryAssembly() is { } entryAssembly)
        {
            builder.AddUserSecrets(entryAssembly);
        }

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
            Describe = parser.GetValue(parsedArgs, "describe", false)
        };
#if DEBUG
        serverArgs = serverArgs with { FindAvailablePort = parser.GetValue(parsedArgs, "find-available-port", true) };
#else
        serverArgs = serverArgs with { FindAvailablePort = parser.GetValue(parsedArgs, "find-available-port", false) };
#endif
        return serverArgs;
    }
}