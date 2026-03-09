---
searchHints:
  - upgrade
  - update
  - version
  - migrate
  - latest
---

# Ivy Upgrade

<Ingress>
Upgrade your existing Ivy project to the latest version of the Ivy framework with a single command.
</Ingress>

The `ivy upgrade` command updates your Ivy project to the latest available version. It modifies your project file (`.csproj`) to reference the newest Ivy packages, ensuring you have access to the latest features, bug fixes, and improvements.

## Basic Usage

```terminal
>ivy upgrade
```

This command will:

- Detect the current Ivy version in your project
- Fetch the latest available Ivy version
- Update all Ivy package references in your `.csproj` file
- Optionally commit the changes to Git

## Command Options

`--verbose` or `-v` - Enable verbose logging for detailed output during the upgrade process.

```terminal
>ivy upgrade --verbose
```

`--ignore-git` - Skip Git checks and commit. By default, Ivy verifies your Git status and commits the upgrade changes automatically.

```terminal
>ivy upgrade --ignore-git
```

## What to Expect

When you run the command, Ivy will update the package references in your project file. A successful upgrade looks like this:

```terminal
>ivy upgrade
Upgrading Ivy from 1.2.14 to 1.2.15...
Updated package references in YourProject.csproj
Ivy upgrade complete!
```

<Callout Type="Tip">
After upgrading, run `ivy run` to verify that your project builds and runs correctly with the new version.
</Callout>

## Prerequisites

Before running `ivy upgrade`, ensure you have:

1. **Ivy Project** - Must be run in a valid Ivy project directory (created with `ivy init`)
2. **Git** - A clean Git working tree (unless using `--ignore-git`)

## Troubleshooting

**Build Errors After Upgrade** - If you encounter build errors after upgrading, check the [release notes](https://github.com/Ivy-Interactive/Ivy-Framework/releases) for any breaking changes. You can also use the `ivy fix` command to automatically resolve common issues.

```terminal
>ivy fix
```

**Git Errors** - If Git checks fail, ensure you have committed or stashed all changes before upgrading. Alternatively, use `--ignore-git` to skip Git checks.

```terminal
>ivy upgrade --ignore-git
```

## Related Commands

| Command | Description |
| :--- | :--- |
| `ivy init` | [Create a new Ivy project](02_Init.md) |
| `ivy run` | [Run your application locally](03_Run.md) |
| `ivy fix` | Fix build and runtime issues |
| `ivy deploy` | [Deploy your application](06_Deployment/01_DeploymentOverview.md) |
