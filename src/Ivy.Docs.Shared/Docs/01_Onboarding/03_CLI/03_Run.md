---
searchHints:
  - run
  - start
  - serve
  - dev
  - watch
  - hot-reload
---

# Ivy Run

<Ingress>
Run your Ivy application locally with hot reload and automatic rebuilds.
</Ingress>

The `ivy run` command is your primary development tool. It starts your project in a live environment that monitors your source code, automatically applying changes or rebuilding as needed. Under the hood, it leverages `dotnet watch` to ensure your development loop is fast and uninterrupted. See [Program](../02_Concepts/01_Program.md) for server and startup configuration.

## Basic Usage

```terminal
>ivy run
```

By default, this starts your application on port **5010**.

## Options

You can also run the command with various options to customize its behavior:

| Option | Description | Example |
| :--- | :--- | :--- |
| `--port <PORT>` | Specify a custom port (default: 5010). | `ivy run --port 8080` |
| `--browse` | Open default browser on start. | `ivy run --browse` |
| `--app <NAME>` | Run a specific [app](../02_Concepts/10_Apps.md) in a multi-app project. | `ivy run --app Admin` |
| `--describe` | Show application metadata without starting. | `ivy run --describe` |
| `--verbose` | Enable detailed logging for debugging. | `ivy run --verbose` |
| `--silent` | Start without the welcome banner. | `ivy run --silent` |

## Configuring the Port

By default, Ivy starts on port **5010**. There are several ways to change it:

### CLI Flag

The simplest approach—pass the `--port` flag:

```terminal
>ivy run --port 5011
```

### Server Configuration in Code

Set the port directly in `Program.cs` (or in a [file-based app](../02_Concepts/19_FileBasedApps.md)):

```csharp
var server = new Server(new ServerArgs { Port = 5011 });
```

This is the recommended approach when running with `dotnet run` or `dotnet watch` directly, since those commands do not support the `--port` flag.

### Environment Variable

Set the `PORT` environment variable before starting the app:

```terminal
>set PORT=5011
>dotnet run
```

This works with any launch method (`ivy run`, `dotnet run`, file-based apps).

See [Program](../02_Concepts/01_Program.md) for more details on server configuration.

## Conflict Resolution & Debugging

| Flag | Description |
| :--- | :--- |
| `--i-kill-for-this-port` | Forcefully kills any process currently using the target port. |
| `--find-available-port` | Automatically searches for the next free port if the target is taken. |
| `--watch-verbose` | Enables verbose output specifically for the file watcher. |

## Development Features

### Hot Reload & Auto-Recovery
Ivy enables **Hot Reload** by default. When you modify method logic, UI layouts, or properties, changes are injected instantly without losing application state (`🔥 Hot reload succeeded`).

For structural changes—such as modifying constructors, changing inheritance, or adding NuGet packages—Ivy usually requires a full restart. It handles this automatically: detecting the change, rebuilding, and restarting the process.

If you save code with build errors, `ivy run` pauses and waits. Simply fix the error and save again to resume; there is no need to stop and restart the command manually.

### Interactive Controls
Control the running application directly from your terminal:
- **Ctrl+R**: Force a manual restart.
- **Ctrl+C**: Gracefully shut down.

## What to Expect
When you run the command, you'll see status messages from the watcher. A successful startup looks like this:

```terminal
dotnet watch 🔥 Hot reload enabled.
dotnet watch 💡 Press Ctrl+R to restart.
dotnet watch 🔨 Building /path/to/Project.csproj ...
dotnet watch 🔨 Build succeeded
Ivy is running on http://localhost:5010
```

## Common Scenarios

**Run on a different port and open browser:**
```terminal
>ivy run --port 3000 --browse
```

**Handle a port conflict by killing the old process:**
```terminal
>ivy run --port 5010 --i-kill-for-this-port
```

**Run a specific [app](../02_Concepts/10_Apps.md) (for multi-app solutions):**
```terminal
>ivy run --app Dashboard
```

## Troubleshooting

- **Port in use:** Use `--find-available-port` to let Ivy pick a free port, or `--i-kill-for-this-port` to claim the specific one you want.
- **Hot Reload fails:** Some complex edits can't be hot-reloaded. Press **Ctrl+R** to force a fresh build.
- **Build errors:** Read the terminal output for compiler errors. The watcher will resume automatically once the file is fixed.

## Related Commands

| Command | Description |
| :--- | :--- |
| `ivy init` | [Create a new Ivy project](02_Init.md) |
| `ivy app create` | Create new apps |
| `ivy fix` | Fix build and runtime issues |
| `ivy deploy` | [Deploy your application](06_Deployment/01_DeploymentOverview.md) |
| `ivy db add` | [Add database connections](05_DatabaseIntegration/01_DatabaseOverview.md) |
| `ivy auth add` | [Add authentication](04_Authentication/01_AuthenticationOverview.md) |
