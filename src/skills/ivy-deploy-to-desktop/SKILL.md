---
name: ivy-deploy-to-desktop
description: >
  Deploy an Ivy project as a standalone desktop application. Use when the user asks to
  publish, deploy, package, or export their Ivy app as a desktop app, native app,
  executable, or .exe. Scaffolds a desktop wrapper project using Ivy.Desktop, generates
  a Program.cs, publishes with dotnet, and copies the executable to the user's desktop.
allowed-tools: Bash(dotnet:*) Bash(ivy:*) Read Write Edit Glob Grep
effort: high
disable-model-invocation: true
---

# ivy-deploy-to-desktop

Deploy an Ivy project as a standalone desktop application using Ivy.Desktop.

## Pre-flight: Read Learnings

If the file `.ivy/learnings/ivy-deploy-to-desktop.md` exists in the project directory, read it first and apply any lessons learned from previous runs of this skill.

## Workflow

1. **Verify the project** -- Confirm the working directory is a valid Ivy project and that it builds successfully with `dotnet build`. If it does not build, stop and ask the user to fix build errors first.

2. **Determine the project namespace** -- Find the root namespace from the `.csproj` file or existing source files.

3. **Scaffold the desktop project** -- Create a desktop wrapper project under `.ivy/publish/[Namespace].Desktop/`. This is a separate .NET project that references the main Ivy project and adds the Ivy.Desktop package.

4. **Generate Program.cs** -- Write a `Program.cs` for the desktop wrapper following the rules in the generation guide below.

5. **Publish the application** -- Run `dotnet publish -c Release` on the desktop project to produce a self-contained executable.

6. **Copy to desktop** -- Place the published executable on the user's desktop so it is ready to run.

## Step 4: Generating Program.cs

Write a `Program.cs` for the desktop project (`[Namespace].Desktop`) at `.ivy/publish/[Namespace].Desktop/Program.cs`.

Before generating, read the original web project's `Program.cs` and list the files in the project's `Apps/` directory.

The generated Program.cs must:

1. Use `Ivy` and `Ivy.Desktop` namespaces.
2. Create a `new Server(new ServerArgs { Silent = true })`.
3. **Assembly references** -- For `AddAppsFromAssembly` and `AddConnectionsFromAssembly`, you MUST pass an explicit assembly reference because the entry assembly in the desktop project is different from the main app assembly. Find a class from the original project (e.g., an `[App]` class) and use `typeof(ThatClass).Assembly` as the argument:
   ```csharp
   var targetAssembly = typeof(MyNamespace.Apps.SomeAppClass).Assembly;
   server.AddAppsFromAssembly(targetAssembly);
   server.AddConnectionsFromAssembly(targetAssembly);
   ```
4. Preserve any other service registrations from the original (e.g. `server.UseCulture(...)`, any `server.Add*` calls).
5. **Chrome handling:**
   - Count the number of apps in the `Apps/` directory.
   - If there are **multiple apps**, preserve the chrome configuration (e.g. `UseAppShell`) in the desktop version so users can navigate between apps via the sidebar.
   - If there is only a **single app**, do NOT add `UseAppShell`. Use `server.UseDefaultApp(typeof(TheApp))` instead. Single-app desktop applications should not have a sidebar.
6. **Window sizing** -- Choose appropriate `.Size(width, height)` values for the `DesktopWindow` based on the apps found in `Apps/`:
   - Multi-app projects with chrome/sidebar: `1280, 800` or larger
   - Single-app dashboards or data-heavy apps: `1200, 800`
   - Single-app form/tool apps (calculators, converters, simple utilities): `800, 600`
   - Single-app narrow/focused apps (chat, single-column): `500, 700`
   - When unsure, default to `1024, 768`
7. Replace the web server startup with:
   ```csharp
   return new DesktopWindow(server)
       .Title("[AppTitle]")
       .Size(WIDTH, HEIGHT)  // Use the dimensions chosen in step 6
       .Run();
   ```
8. Do NOT include any web host building, `app.Run()`, `builder.Build()`, Kestrel, or HTTP pipeline code.

**CRITICAL:** Write ONLY to `.ivy/publish/[Namespace].Desktop/Program.cs`. Do NOT create any files in the main project directory. Do NOT create a `.Desktop` subfolder in the main project. The desktop project lives under `.ivy/publish/[Namespace].Desktop/` -- all desktop files must go there.

Output ONLY raw C# code, no markdown fences or explanation.

## Troubleshooting

If the publish step fails:

1. Ensure the project builds successfully with `dotnet build`.
2. Check that the Ivy.Desktop NuGet package is available.
3. Verify you have the .NET 10 SDK installed.
4. If the error mentions "Ambiguous project name", create a `Directory.Build.props` in the desktop project directory with content `<Project></Project>` to isolate the project from the parent directory tree.

### Recovery Steps

1. Diagnose using the troubleshooting steps above to identify the root cause.

2. After fixing the underlying issue, choose ONE of these approaches:

   **Option A: Retry from scratch** (recommended for transient failures like NuGet package issues or temporary build failures) -- re-run the full workflow from step 1.

   **Option B: Manual desktop publish** (recommended for persistent workflow failures):
   - Run `dotnet publish -c Release -r <rid>` (e.g., `-r win-x64`, `-r osx-arm64`, `-r linux-x64`)
   - Locate the published executable in `bin/Release/net10.0/{rid}/publish/`
   - Copy the executable to the user's desktop (see "Desktop Copy" below)

   Use `ivy docs` to find deployment patterns: "How do I publish an Ivy app as a desktop application?"

3. Do NOT skip the desktop copy step -- the executable must be accessible on the desktop for the user.

### Desktop Copy

If you recover manually, you MUST copy the published executable to the user's desktop:
- Publish location: `bin/Release/net10.0/{rid}/publish/`
- Desktop folder: Use the system desktop path (e.g., `C:\Users\{user}\Desktop\`)
- Copy the `.exe` file (Windows) or the binary (macOS/Linux) to the desktop

## Completion

When the publish succeeds, inform the user:
- The application name
- The output location (on their desktop)
- That it is ready to run

## Post-run: Evaluate and Improve

After completing the task:

1. **Evaluate**: Did the build succeed? Were there compilation errors, unexpected behavior, or manual corrections needed during this run?
2. **Update learnings**: If anything required correction or was surprising, append a concise entry to `.ivy/learnings/ivy-deploy-to-desktop.md` (create the file and `.ivy/learnings/` directory if they don't exist). Each entry should note: the date, what went wrong, why, and what to do differently next time.
3. **Skip if clean**: If everything succeeded without issues, do not update the learnings file.
