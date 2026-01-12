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

The `ivy run` command starts your Ivy project with hot reload enabled, allowing you to see changes instantly as you develop. It uses `dotnet watch` under the hood to automatically detect file changes and apply them without restarting your application.

## Basic Usage

```terminal
>ivy run
```

This command will:

- Start the Ivy application from the current directory on the default port (5010)
- Enable hot reload for instant updates
- Monitor for file changes and restart automatically if a change is not compatible with hot reload
- Provide keyboard shortcuts for manual control

### Command Options

`--port <PORT>` - Specify the port to run your application on. Default is 5010.

```terminal
>ivy run --port 5020
```

`--browse` - Automatically open your default browser when the application starts.

```terminal
>ivy run --browse
```

`--app <APP_NAME>` - Specify which app to run in a multi-app project.

```terminal
>ivy run --app Dashboard
```

`--describe` - Show detailed information about the application before starting.

```terminal
>ivy run --describe
```

`--i-kill-for-this-port` - Kill any process using the specified port before starting. Useful when a previous instance didn't shut down cleanly.

```terminal
>ivy run --i-kill-for-this-port --port 5010
```

`--find-available-port` - Automatically find an available port if the specified port is in use.

```terminal
>ivy run --find-available-port
```

`--watch-verbose` - Enable verbose output from dotnet watch for debugging.

```terminal
>ivy run --watch-verbose
```

`--verbose` - Enable verbose output for detailed logging from the Ivy server and your app.

```terminal
>ivy run --verbose
```

`--silent` - Suppress the startup message when your application starts.

```terminal
>ivy run --silent
```

## Interactive Controls

While your application is running, you can use keyboard shortcuts to control it:

**Ctrl+R** - Manually restart the application. Useful when you want to force a full restart.

```terminal
dotnet watch 🔄 Restart requested.
```

**Ctrl+C** - Gracefully shut down the application and exit.

```terminal
dotnet watch 🛑 Shutdown requested.
```

## Hot Reload

Ivy run automatically applies code changes without restarting your application when possible. When you save a file, you'll see:

```terminal
dotnet watch 🔥 Hot reload succeeded
```

### When Hot Reload Works

Hot reload works for most common code changes:

- Modifying method implementations
- Adding new methods or properties
- Changing UI layouts and styling
- Updating view logic

### When a Restart is Required

Some changes require a full application restart:

- Adding or removing constructor parameters
- Changing class inheritance
- Modifying Program.cs startup configuration
- Adding new NuGet packages

When a restart is needed, Ivy will automatically rebuild and restart your application.

## Automatic Recovery

If your code has build errors, Ivy run will:

1. Display the build error messages
2. Wait for you to fix the issues
3. Automatically rebuild when you save the corrected files
4. Resume running once the build succeeds

```terminal
dotnet watch 🔨 Build FAILED.
dotnet watch ⏳ Waiting for a file to change before restarting...
# Fix your code and save
dotnet watch 🔨 Build succeeded
```

## Port Management

### Default Port

By default, Ivy applications run on port 5010:

```terminal
Ivy is running on http://localhost:5010
```

### Custom Ports

Specify a custom port with the `--port` option:

```terminal
>ivy run --port 8080
Ivy is running on http://localhost:8080
```

### Port Conflicts

If the specified port is already in use, you have two options:

**Option 1**: Kill the existing process

```terminal
>ivy run --i-kill-for-this-port --port 5010
```

**Option 2**: Find an available port automatically

```terminal
>ivy run --find-available-port
Ivy is running on http://localhost:5011
```

## Multi-App Projects

If your project contains multiple apps, you can specify which one to run:

```terminal
>ivy run --app AdminDashboard
```

This is useful for large projects where different apps serve different purposes (e.g., public site, admin panel, API).

## Examples

**Basic Development**

```terminal
>ivy run
```

**Custom Port with Browser**

```terminal
>ivy run --port 3000 --browse
```

**Run Specific App**

```terminal
>ivy run --app Analytics --port 5020
```

**Development with Verbose Logging**

```terminal
>ivy run --verbose --watch-verbose
```

## Output Messages

When running your application, you'll see various status messages from `dotnet watch`:

**Starting Up**

```terminal
dotnet watch 🔥 Hot reload enabled.
dotnet watch 💡 Press Ctrl+R to restart.
dotnet watch 🔨 Building /path/to/Project.csproj ...
dotnet watch 🔨 Build succeeded
Ivy is running on http://localhost:5010
```

**Hot Reload Success**

```terminal
dotnet watch 🔥 Hot reload succeeded
```

**Restart Required**

```terminal
dotnet watch 🔥 Restart is needed to apply the changes.
dotnet watch ⌚ [Project] Exited
dotnet watch 🔨 Building ...
```

**Build Errors**

```terminal
dotnet watch 🔨 Build FAILED.
dotnet watch ❌ error CS0103: The name 'variable' does not exist
```

**Shutdown**

```terminal
dotnet watch 🛑 Shutdown requested.
dotnet watch ⌚ [Project] Exited
```

## Troubleshooting

**Port Already in Use**

If you see an error about the port being in use:

```terminal
>ivy run --i-kill-for-this-port
```

Or use a different port:

```terminal
>ivy run --port 5011
```

**Application Not Hot Reloading**

If hot reload isn't working:

1. Check that your changes are supported by hot reload
2. Try a manual restart with Ctrl+R
3. Shutdown the application completely with Ctrl+C and run again

**Build Errors on Startup**

If your application fails to build:

1. Review the error messages in the terminal
2. Fix the issues in your code
3. Save the file - the build will restart automatically
4. If problems persist, run `ivy fix` to diagnose issues

```terminal
>ivy fix
```

## Related Commands

- `ivy init` - Create a new Ivy project
- `ivy app create` - Create new apps
- `ivy fix` - Fix build and runtime issues
- `ivy deploy` - Deploy your application
- `ivy db add` - Add database connections
- `ivy auth add` - Add authentication
