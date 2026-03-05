---
searchHints:
  - init
  - create
  - scaffold
  - new-project
  - setup
  - initialize
---

# Ivy Init - Getting Started

<Ingress>
Quickly scaffold new Ivy projects with the necessary structure, configuration files, and boilerplate code using the init command.
</Ingress>

The `ivy init` command creates a new Ivy project with the necessary structure and configuration files to get you started quickly. See [Program](../02_Concepts/01_Program.md) for how the generated entry point runs your app.

## Basic Usage

```terminal
>ivy init
```

This command will:

- Create a new Ivy project in the current directory
- Set up the basic project structure
- Generate necessary configuration files

### Command Options

`--namespace <NAMESPACE>` or `-n <NAMESPACE>` - Specify the namespace for your Ivy project. If not provided, Ivy will suggest a namespace based on the folder name.

```terminal
>ivy init --namespace MyCompany.MyProject
```

`--dangerous-clear` - Clear the current folder before creating the new project. **Use with caution!**

```terminal
>ivy init --dangerous-clear
```

`--dangerous-overwrite` - Overwrite existing files in the current folder. **Use with caution!**

```terminal
>ivy init --dangerous-overwrite
```

`--verbose` - Enable verbose output for detailed logging during initialization.

```terminal
>ivy init --verbose
```

`--hello` - Include a simple demo app in the new project to help you get started.

```terminal
>ivy init --hello
```

`--script` - Create a simple Ivy script file instead of a full project. Perfect for quick prototyping or single-file applications.

```terminal
>ivy init --script
```

`--template <TEMPLATE>` or `-t <TEMPLATE>` - Use a specific template for the new project.

```terminal
>ivy init --template my-template
```

`--select-template` - Interactively select a template from available options.

```terminal
>ivy init --select-template
```

`--cursor` - Install Cursor MCP integration after project creation.

```terminal
>ivy init --cursor
```

`--claude` - Install Claude Code MCP integration after project creation.

```terminal
>ivy init --claude
```

`--ignore-git` - Skip Git checks and commit during initialization.

```terminal
>ivy init --ignore-git
```

`--prerelease` - Include prerelease versions when fetching the latest Ivy version.

```terminal
>ivy init --prerelease
```

`--yes-to-all` - Skip all prompts and use default values. Useful for automated scripts.

```terminal
>ivy init --yes-to-all
```

### Interactive Mode

When you run `ivy init` without specifying a namespace, Ivy will prompt you to enter one:

```terminal
Namespace for the new Ivy project: [suggested-namespace]
```

Ivy will suggest a namespace based on your current folder name. You can accept the suggestion or enter a custom namespace.

### Project Structure

After running `ivy init`, your project will have the following structure. The generated [Program.cs](../02_Concepts/01_Program.md) is the application entry point.

```text
YourProject/
├── Program.cs              # Main project entry point
├── YourProject.csproj      # .NET project file
├── README.md               # Project documentation
└── .gitignore              # Git ignore file
```

### Generated Files

### Program.cs

The main [entry point](../02_Concepts/01_Program.md) for your Ivy project:

```csharp
var server = new Server();
server.UseCulture("en-US");
#if !DEBUG
server.UseHttpRedirection();
#endif
#if DEBUG
server.UseHotReload();
#endif
server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();
server.UseHotReload();
var chromeSettings = new ChromeSettings().UseTabs(preventDuplicates: true);
server.UseChrome(chromeSettings);
await server.RunAsync();
```

The template configures [Chrome](../02_Concepts/11_Chrome.md) for the browser UI.

### Prerequisites

Before running `ivy init`, ensure you have:

1. **.NET SDK** installed (version 8.0 or later)
2. **Git** installed (optional, but recommended)
3. **Empty directory** or use `--dangerous-clear`/`--dangerous-overwrite`

### Validation

Ivy performs several validations during initialization:

- **Directory Check**: Ensures the target directory is empty (unless using overwrite options)
- **Namespace Validation**: Validates the provided namespace format
- **Git Status**: Checks for uncommitted changes if Git is initialized
- **.NET Tools**: Ensures required .NET tools are installed

### Error Handling

**Empty Directory Required** - If the current directory is not empty, Ivy will show an error:

```terminal
The current folder is not empty. Please clear the folder or use the --dangerous-clear option or --dangerous-overwrite
```

**Invalid Namespace** - If you provide an invalid namespace, Ivy will prompt you to enter a valid one:

```terminal
Invalid 'invalid-namespace' namespace. Please enter a valid namespace.
```

### Next Steps

After initializing your project:

1. **Add a database connection**: `ivy db add` — see [Database Overview](05_DatabaseIntegration/01_DatabaseOverview.md)
2. **Add authentication**: `ivy auth add` — see [Authentication Overview](04_Authentication/01_AuthenticationOverview.md)
3. **Create an app**: `ivy app create` — see [Apps](../02_Concepts/10_Apps.md)
4. **Deploy your project**: `ivy deploy` — see [Deployment Overview](06_Deployment/01_DeploymentOverview.md)

## Examples

**Basic Project Initialization**

```terminal
>mkdir MyIvyProject
>cd MyIvyProject
>ivy init
```

**Project with Custom Namespace**

```terminal
>ivy init --namespace AcmeCorp.InventorySystem
```

**Project with Demo App**

```terminal
>ivy init --helloworld --namespace MyDemoProject
```

**Verbose Initialization**

```terminal
>ivy init --verbose --namespace MyProject
```

<Callout Type="Tip">
 The CLI plays a success sound when operations complete. Use `--silent` to disable audio notifications.
</Callout>

### Troubleshooting

**Permission Issues** - If you encounter permission issues, ensure you have write access to the current directory.

**NET Tools Not Found** - If required .NET tools are missing, Ivy will attempt to install them automatically. You may need to run:

```terminal
>dotnet tool install -g dotnet-ef
>dotnet tool install -g dotnet-user-secrets
```

**Git Issues** - If Git is not installed or configured, Ivy will still create the project but may skip some Git-related operations.

**Build Errors** - If you encounter build errors, you can use the `ivy fix` command to automatically resolve common issues. See also [Program](../02_Concepts/01_Program.md) for startup and configuration. The default timeout is 360 seconds (6 minutes).

```terminal
>ivy fix
```

Use the `--timeout` option to specify a custom timeout in seconds:

```terminal
>ivy fix --timeout 600
```

Use **Claude Code** for debugging:

```terminal
>ivy fix --use-claude-code
```

**Set environment** variable for Claude Code

```terminal
>export IVY_FIX_USE_CLAUDE_CODE=true
>ivy fix
```

**Enable telemetry upload**

<Callout Type="Warning">
Telemetry Upload Details: When telemetry upload is enabled, the `ivy fix` command will upload an anonymized snapshot of your project (excluding .git, bin, and obj folders) for analysis. This helps the Ivy team understand common build issues and improve the fix command's effectiveness. The telemetry upload only includes source code files, has a 50MB size limit, and is completely optional and disabled by default.
</Callout>

```terminal
>export IVY_FIX_UPLOAD_TELEMETRY=true
>ivy fix
```

**Debug commands** to manage settings

```terminal
>ivy debug enable-ivy-fix-upload-telemetry
>ivy debug disable-ivy-fix-upload-telemetry
```

### Creating Apps

Create new [apps](../02_Concepts/10_Apps.md) using AI assistance. The default timeout is 360 seconds (6 minutes).

```terminal
>ivy app create MyApp
```

Use the `--timeout` option to specify a custom timeout in seconds:

```terminal
>ivy app create MyApp --timeout 600
```

### Removing Apps

**Remove a specific app by name**

```terminal
>ivy app remove --name MyApp
```

**Interactive mode** - select from a list of existing apps

```terminal
>ivy app remove
```

**Remove all** apps at once

```terminal
>ivy app remove --all
```

### Related Commands

- `ivy db add` - Add database connections
- `ivy auth add` - Add authentication providers
- `ivy app create` - Create apps
- `ivy app remove` - Remove apps
- `ivy deploy` - Deploy your project
