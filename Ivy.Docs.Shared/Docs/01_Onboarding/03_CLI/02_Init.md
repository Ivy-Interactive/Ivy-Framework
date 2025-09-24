# Ivy Init - Getting Started

<Ingress>
Quickly scaffold new Ivy projects with the necessary structure, configuration files, and boilerplate code using the init command.
</Ingress>

The `ivy init` command creates a new Ivy project with the necessary structure and configuration files to get you started quickly.

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

`--helloworld` or `--hello` - Include a simple demo app in the new project to help you get started.

```terminal
>ivy init --helloworld
```

### Interactive Mode

When you run `ivy init` without specifying a namespace, Ivy will prompt you to enter one:

```terminal
Namespace for the new Ivy project: [suggested-namespace]
```

Ivy will suggest a namespace based on your current folder name. You can accept the suggestion or enter a custom namespace.

### Project Structure

After running `ivy init`, your project will have the following structure:

```text
YourProject/
├── Program.cs              # Main project entry point
├── YourProject.csproj      # .NET project file
├── GlobalUsings.cs         # Global using statements
├── README.md               # Project documentation
└── .gitignore              # Git ignore file
```

### Generated Files

### Program.cs

The main entry point for your Ivy project:

```csharp
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
var server = new Server();
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

### GlobalUsings.cs

Global using statements for common Ivy namespaces:

```csharp
global using Ivy;
global using Ivy.Apps;
global using Ivy.Auth;
global using Ivy.Chrome;
global using Ivy.Client;
global using Ivy.Core;
global using Ivy.Core.Hooks;
global using Ivy.Helpers;
global using Ivy.Hooks;
global using Ivy.Shared;
global using Ivy.Views;
global using Ivy.Views.Alerts;
global using Ivy.Views.Builders;
global using Ivy.Views.Blades;
global using Ivy.Views.Forms;
global using Ivy.Views.Tables;
global using Ivy.Widgets.Inputs;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using System.Collections.Immutable;
global using System.ComponentModel.DataAnnotations;
global using System.Globalization;
global using System.Reactive.Linq;

namespace YourProject;
```

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

1. **Add a database connection**: `ivy db add`
2. **Add authentication**: `ivy auth add`
3. **Create an app**: `ivy app create`
4. **Deploy your project**: `ivy deploy`

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

**Build Errors** - If you encounter build errors, you can use the `ivy fix` command to automatically resolve common issues:
To use the default AI debugging mode, run:

```terminal
>ivy fix
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

### App Removal Command

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
