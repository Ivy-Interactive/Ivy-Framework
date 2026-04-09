namespace Ivy.Tendril.Services;

public class OnboardingSetupService(IConfigService config, IServiceProvider services) : IOnboardingSetupService
{
    public async Task CompleteSetupAsync(string tendrilHome)
    {
        // Create directory structure
        Directory.CreateDirectory(tendrilHome);
        Directory.CreateDirectory(Path.Combine(tendrilHome, "Inbox"));
        Directory.CreateDirectory(Path.Combine(tendrilHome, "Plans"));
        await FileHelper.WriteAllTextAsync(Path.Combine(tendrilHome, "Plans", ".counter"), "1");
        Directory.CreateDirectory(Path.Combine(tendrilHome, "Trash"));
        Directory.CreateDirectory(Path.Combine(tendrilHome, "Promptwares"));
        if (PromptwareDeployer.IsEmbeddedAvailable())
            PromptwareDeployer.Deploy(Path.Combine(tendrilHome, "Promptwares"));
        Directory.CreateDirectory(Path.Combine(tendrilHome, "Hooks"));

        // Copy template or create basic config
        var projectDir = Path.GetDirectoryName(System.AppContext.BaseDirectory);
        while (projectDir != null && !File.Exists(Path.Combine(projectDir, "example.config.yaml")))
            projectDir = Path.GetDirectoryName(projectDir);

        var exampleConfigPath = projectDir != null
            ? Path.Combine(projectDir, "example.config.yaml")
            : Path.Combine(System.AppContext.BaseDirectory, "example.config.yaml");

        var configPath = Path.Combine(tendrilHome, "config.yaml");

        if (File.Exists(exampleConfigPath))
        {
            var exampleContent = await FileHelper.ReadAllTextAsync(exampleConfigPath);
            await FileHelper.WriteAllTextAsync(configPath, exampleContent);
        }
        else if (!File.Exists(configPath))
        {
            var basicConfig = "codingAgent: claude\n" +
                              "jobTimeout: 30\n" +
                              "staleOutputTimeout: 10\n" +
                              "projects: []\n" +
                              "verifications: []\n";
            await FileHelper.WriteAllTextAsync(configPath, basicConfig);
        }

        // Set environment variable for current session
        Environment.SetEnvironmentVariable("TENDRIL_HOME", tendrilHome);

        // Persist environment variable across restarts
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Environment.SetEnvironmentVariable("TENDRIL_HOME", tendrilHome, EnvironmentVariableTarget.User);
            }
            else
            {
                // Determine shell rc file
                var shell = Environment.GetEnvironmentVariable("SHELL") ?? "";
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var rcFile = shell.EndsWith("/zsh") ? Path.Combine(home, ".zshrc")
                           : shell.EndsWith("/bash") ? Path.Combine(home, ".bashrc")
                           : Path.Combine(home, ".profile");

                var exportLine = $"export TENDRIL_HOME=\"{tendrilHome}\"";

                var content = File.Exists(rcFile) ? await FileHelper.ReadAllTextAsync(rcFile) : "";
                if (!content.Contains(exportLine))
                    await File.AppendAllLinesAsync(rcFile, new[] { "", "# Tendril Home", exportLine });
            }
        }
        catch
        {
            /* Best effort — env var is set for current session regardless */
        }

        // Mark onboarding complete (this reloads config from the file we just wrote)
        config.CompleteOnboarding(tendrilHome);

        // Add pending verification definitions to global config
        var pendingDefinitions = config.GetPendingVerificationDefinitions();
        if (pendingDefinitions != null)
            foreach (var def in pendingDefinitions)
                if (!config.Settings.Verifications.Any(v => v.Name == def.Name))
                    config.Settings.Verifications.Add(def);

        // Add pending project if one was configured
        var pendingProject = config.GetPendingProject();
        if (pendingProject != null)
        {
            config.Settings.Projects.Add(pendingProject);
            config.SaveSettings();
        }

        // Initialize database and start background services now that TendrilHome is set
        BackgroundServiceActivator.Start(services);
    }
}
