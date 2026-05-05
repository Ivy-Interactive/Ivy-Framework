---
name: ivy-create-using-reference-connection
description: >
  Create a connection from the Ivy catalog of pre-built reference connections. Use
  when the user wants to add a reference connection, browse the Ivy catalog, use a
  pre-built connection template, or set up a catalog connection in their Ivy project.
allowed-tools:
  - Bash(dotnet:*)
  - Bash(ivy:*)
  - Read
  - Write
  - Edit
  - Glob
  - Grep
effort: medium
---

# Create a Connection Using a Reference

This skill creates a connection in an Ivy project using a pre-built reference connection from the Ivy catalog. Reference connections provide templates with NuGet packages, secrets configuration, and example implementation files that guide the agent in building the actual connection.

## Pre-flight: Read Learnings

If the file `.ivy/learnings/ivy-create-using-reference-connection.md` exists in the project directory, read it first and apply any lessons learned from previous runs of this skill.

## Prerequisites

- The working directory must be a valid Ivy project.

## Concepts

**Reference connections** are pre-built connection templates maintained in the Ivy catalog. Each reference connection includes:

- **NuGet packages** required for the connection (with optional documentation and GitHub links)
- **Secrets** that need to be configured (API keys, tokens, endpoints)
- **Reference files** containing example implementations that show how to build the connection class, tests, and demo apps

The reference files are exposed as guidance -- they are not copied directly into the project. Instead, you should read them and create a similar implementation adapted to the user's project.

## Workflow

### 1. Select a Reference Connection

If a reference connection name is not already specified, list the available connections from the Ivy catalog.

Ask the user which reference connection they want to use. Present the connection names as options along with their descriptions, services, and tags.

### 2. Download the Reference

Download the selected reference connection from the Ivy catalog. This fetches the reference files and metadata to a local folder.

If the download fails, report the error to the user.

### 3. Collect Secrets

If the reference connection requires secrets (API keys, tokens, etc.), ask the user for each secret value. Each secret has a key and description explaining what it is for.

If no secrets are required, skip to the next step.

### 4. Install and Set Up

This step performs several automated actions:

1. **Store secrets** -- If secrets were collected, initialize dotnet user-secrets and store each secret value. The secret keys use the format `ConnectionName:SecretKey`.

2. **Install NuGet packages** -- Add all required NuGet packages from the reference connection. Run `dotnet restore` after installation.

3. **Create the connection directory** -- Create `Connections/[ConnectionName]/` in the project.

### 5. Build the Connection

Using the reference files as guidance, create the connection implementation:

- Read each reference file to understand the example implementation patterns
- Create a similar implementation adapted to the user's project under `Connections/[ConnectionName]/`
- The implementation should include the connection class, any models, and configuration

**Package Management Rules:**
1. Add `<PackageReference>` elements to the .csproj file
2. Immediately run `dotnet add package [PackageName]` for each package to ensure proper restore
3. Then run `dotnet build` to verify compilation

**Secret Handling Rules:**
- Do NOT run `dotnet user-secrets` commands -- the workflow has already stored all collected secrets automatically.
- If the user specified custom values in their prompt (API keys, tokens, endpoints, etc.), set them as preset values in the connection's `GetSecrets()` method:

```csharp
public Secret[] GetSecrets() =>
[
    new Secret("OpenAI:ApiKey", "sk-user-provided-key"),
    new Secret("OpenAI:Endpoint", "https://custom-endpoint.com/"),
];
```

This applies to ALL user-supplied values including API keys and tokens -- presets are how the system delivers credentials at runtime.

### 6. Verify

Test the connection using the test tool with the connection name. If the test fails, investigate and fix the issue.

### 7. Completion

Report the results:
- Connection name
- NuGet packages installed
- Secrets stored (with prefix `[ConnectionName]:`)
- Reference files that were used as guidance

The reference files remain available for building applications with this connection. Use the Read tool to review any reference file for implementation patterns.

## Post-run: Evaluate and Improve

After completing the task:

1. **Evaluate**: Did the build succeed? Were there compilation errors, unexpected behavior, or manual corrections needed during this run?
2. **Update learnings**: If anything required correction or was surprising, append a concise entry to `.ivy/learnings/ivy-create-using-reference-connection.md` (create the file and `.ivy/learnings/` directory if they don't exist). Each entry should note: the date, what went wrong, why, and what to do differently next time.
3. **Skip if clean**: If everything succeeded without issues, do not update the learnings file.
