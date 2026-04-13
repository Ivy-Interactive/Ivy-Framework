namespace Ivy.Tendril.Services;

public static class PromptwareCommands
{
    /// <summary>
    ///     Handles promptware CLI commands. Returns exit code (0 = success, 1 = error),
    ///     or -1 if the args don't match a promptware command.
    /// </summary>
    public static int Handle(string[] args)
    {
        if (args.Length == 0) return -1;

        return args[0] switch
        {
            "update-promptwares" => UpdatePromptwaresCommand(),
            _ => -1
        };
    }

    private static int UpdatePromptwaresCommand()
    {
        var tendrilHome = Environment.GetEnvironmentVariable("TENDRIL_HOME");
        if (string.IsNullOrEmpty(tendrilHome))
        {
            Console.Error.WriteLine("Error: TENDRIL_HOME environment variable is not set.");
            return 1;
        }

        if (!PromptwareDeployer.IsEmbeddedAvailable())
        {
            Console.Error.WriteLine("Error: No embedded promptwares found in this build.");
            return 1;
        }

        var target = Path.Combine(tendrilHome, "Promptwares");
        Console.WriteLine($"Updating promptwares in {target}...");
        PromptwareDeployer.Deploy(target);
        Console.WriteLine("Done.");
        return 0;
    }
}
