# IvyFrameworkVerification

Test and visually verify Ivy Framework UI changes by creating demo apps and running Playwright tests.

## Context

The firmware header contains:

- **PlanFolder** — path to the plan folder
- **ConfigPath** — absolute path to config.yaml
- **CurrentTime** — current UTC timestamp
- **VerificationDir** — path to write the verification report
- **ArtifactsDir** — path to store test artifacts (screenshots, videos, sample apps)
- **IvyFrameworkPath** — pre-resolved path to the Ivy Framework source (worktree or main repo). The launcher has already pre-built the frontend and Ivy.dll from this path. Use this for ProjectReference paths — do NOT rebuild the frontend yourself.

## Execution Steps

### 1. Read Plan

- Read `plan.yaml` from the plan folder
- Read the latest revision from `revisions/` to understand what changed
- Determine if the changes affect visual/UI behavior

If the changes are non-visual (docs, analyzers, refactoring, code-only fixes), write a report noting "No visual verification needed" and exit successfully.

### 2. Research

- Read `Memory/IvyFrameworkGotchas.md` for known API issues and workarounds
- Read `Memory/PlaywrightKnowledge-Index.md` for an overview of the split knowledge files
- Always read the critical files:
  - `Memory/PlaywrightKnowledge-Process.md` — mandatory for all tests
  - `Memory/PlaywrightKnowledge-Gotchas.md` — avoid known crash patterns
- Read additional files based on the feature being verified (identified in Step 1):
  - For new test projects or framework setup: `Memory/PlaywrightKnowledge-Framework.md`
  - For widget testing: `Memory/PlaywrightKnowledge-Widgets.md`
  - For locator/assertion work: `Memory/PlaywrightKnowledge-Locators.md`
  - For DOM/rendering issues: `Memory/PlaywrightKnowledge-DOM.md`
  - For test structure guidance: `Memory/PlaywrightKnowledge-Testing.md`
- Read the Ivy Framework AGENTS.md: `~/git/ivy/Ivy-Framework/AGENTS.md`
- Read relevant source code for the changed feature from `~/git/ivy/Ivy-Framework/src/`
- Read existing samples: `~/git/ivy/Ivy-Framework/src/Ivy.Samples.Shared/Apps/`

### 3. Verify Completeness

Check that required companion artifacts exist for the feature being verified:

1. **Identify the feature type** from the plan revision:
   - Widget (new or modified widget class)
   - Hook (new or modified hook)
   - Concept (new layout, form feature, navigation, etc.)
   - Bugfix/Refactor (internal change, no new public API)

2. **Sample App**: Search `~/git/ivy/Ivy-Framework/src/Ivy.Samples.Shared/Apps/` for files that demonstrate the feature:
   - Widgets → search by widget class name in `Apps/Widgets/`
   - Hooks → search by hook name (e.g. `UseQuery`) across all `Apps/`
   - Concepts → search by concept name across `Apps/Concepts/`
   - Bugfix/Refactor → skip (no sample expected)

3. **Documentation Page**: Search `~/git/ivy/Ivy-Framework/src/Ivy.Docs.Shared/Docs/` for documentation:
   - Widgets → `Docs/02_Widgets/`
   - Hooks → `Docs/03_Hooks/`
   - Concepts → `Docs/01_Onboarding/02_Concepts/`
   - Other → broad search across all `Docs/`
   - Bugfix/Refactor → skip (no doc expected)

4. If the plan's commits modified an existing Sample or Doc file, verify the changes are present in the worktree.

Record results for the report. For missing artifacts on new features, flag as a warning.

### 4. Create Sample Project

Create everything directly in `<ArtifactsDir>/sample/` so the plan folder is self-contained and runnable.

**Important: Use the `IvyFrameworkPath` firmware value** for the ProjectReference path. The launcher script has already pre-built the frontend and Ivy.dll from the correct source (worktree or main repo). Do NOT rebuild the frontend yourself — no `vp build`, no `npx vite build`, no `rm -rf obj/Debug`.

**`<ArtifactsDir>/sample/Sample.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="<IvyFrameworkPath>\src\Ivy\Ivy.csproj" />
    <ProjectReference Include="<IvyFrameworkPath>\src\Ivy.Analyser\Ivy.Analyser.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
  </ItemGroup>
</Project>
```

**Note:** Always use the filename `Sample.csproj` (not `<FeatureName>.csproj`) to ensure reruns with "Suggested Changes" overwrite the same file instead of creating multiple .csproj files.

**`<ArtifactsDir>/sample/Program.cs`:**

```csharp
using Ivy;
using System.Reflection;

var server = new Server();
server.AddAppsFromAssembly(Assembly.GetExecutingAssembly());
server.UseAppShell();
await server.RunAsync();
```

### 5. Create Demo Apps

Create multiple `.cs` app files exercising the feature:

- **BasicApp** — Simplest usage, core functionality
- **PropsApp** — All props/configuration options with visible output
- **EventsApp** — All events with state feedback showing the event fired
- **IntegrationApp** — Feature combined with other Ivy widgets
- **EdgeCasesApp** — Empty values, large data, rapid interactions

Each app must:

- Inherit from `ViewBase` (NOT `AppBase`)
- Have `[App]` attribute with descriptive title and appropriate icon
- Show clear labels for what each section tests
- Display state changes visibly so Playwright can verify them

### 6. Build and Verify

From `<ArtifactsDir>/sample/`:

Before building, kill any leftover processes from previous runs that may lock DLLs (scoped to this plan's artifacts only):

```bash
powershell.exe -NoProfile -Command "Get-Process -ErrorAction SilentlyContinue | Where-Object { \$_.Path -and \$_.Path -like '<ArtifactsDir>*' } | ForEach-Object { Write-Host \"Killing \$(\$_.ProcessName) (PID \$(\$_.Id))\"; \$_ | Stop-Process -Force -ErrorAction Stop }; Start-Sleep -Milliseconds 2000"
```

Use the pre-flight build validation tool:

```bash
pwsh -NoProfile -File "<PromptwareDir>/Tools/Test-SampleBuild.ps1" -SampleProjectDir "<ArtifactsDir>/sample"
```

The tool runs `dotnet build` and `dotnet run --describe`, returning JSON with `success`, `apps`, and `errors` fields. If it fails, fix the **sample project code** (not the framework build) and re-run. The framework is already pre-built by the launcher.

If the tool is unavailable, fall back to:

```bash
dotnet build
dotnet run --describe
```

Fix any compilation errors. Iterate until build succeeds.

### 7. Create Playwright Tests

Create `<ArtifactsDir>/sample/.ivy/tests/` directory with:

**package.json** — minimal, with `@playwright/test` dependency

**playwright.config.ts** — Chromium only, single worker, no retries, viewport `{ width: 1920, height: 1920 }` (set in both `use` and `projects[0].use`), uses `process.env.APP_PORT`

**IMPORTANT:** Screenshots must be written to `<ArtifactsDir>/screenshots/` (sibling to `sample/`), not inside `sample/`. Since `projectRoot` resolves to `<ArtifactsDir>/sample/`, use `path.resolve(projectRoot, '..', 'screenshots')` (single `..`) — NOT double `..` which goes above `<ArtifactsDir>`.

**test-utils.ts** — process tracking utility for cleanup on timeout/crash:

```typescript
import { ChildProcess } from 'child_process';

const activeProcesses = new Set<ChildProcess>();

/**
 * Track a spawned process so it can be killed on timeout/crash.
 */
export function trackProcess(proc: ChildProcess) {
  activeProcesses.add(proc);
  proc.on('exit', () => activeProcesses.delete(proc));
}

/**
 * Kill all tracked processes. Called by cleanup handlers.
 */
export function killAllTrackedProcesses() {
  activeProcesses.forEach(proc => {
    if (!proc.killed) {
      try {
        if (process.platform === 'win32') {
          // Windows: taskkill with /F to force immediate termination
          require('child_process').execSync(`taskkill /pid ${proc.pid} /F /T`, {
            stdio: 'ignore',
          });
        } else {
          proc.kill('SIGKILL');
        }
      } catch (e) {
        // Process may have already exited
      }
    }
  });
  activeProcesses.clear();
}

// Register global cleanup handlers for abnormal termination
process.on('SIGINT', () => {
  console.log('\nTest runner interrupted (SIGINT), cleaning up processes...');
  killAllTrackedProcesses();
  process.exit(130);
});

process.on('SIGTERM', () => {
  console.log('\nTest runner terminated (SIGTERM), cleaning up processes...');
  killAllTrackedProcesses();
  process.exit(143);
});

process.on('uncaughtException', (err) => {
  console.error('Uncaught exception:', err);
  killAllTrackedProcesses();
  process.exit(1);
});

// Best-effort cleanup on normal exit (afterAll should have already run)
process.on('exit', () => {
  killAllTrackedProcesses();
});
```

**One `.spec.ts` per app:**

- Import `trackProcess` and `killAllTrackedProcesses` from `./test-utils`
- `beforeAll`: find free port, spawn `dotnet run -- --port <port>`, **call `trackProcess(proc)`**, wait for HTTP 200
- `afterAll`: kill process with `killAllTrackedProcesses()` (also kills any other tracked processes)
- Set `test.setTimeout(60000)` (60s) to catch hung tests before Playwright's default timeout
- Test each app at `https://localhost:<port>/<app-id>?shell=false`
- Take screenshots directly to `<ArtifactsDir>/screenshots/` with descriptive names. **Before taking each screenshot, check if the page has meaningful content (visible text > 20 chars or > 5 visible elements). Skip screenshots of empty/blank pages** — these add no verification value. Use a `takeScreenshotIfNotEmpty()` helper (see PlaywrightKnowledge.md)
- Capture browser console logs → `<ArtifactsDir>/tests/console.log`
- Capture backend stdout/stderr → `<ArtifactsDir>/tests/backend.log`

**Test coverage must verify:**

1. Feature renders correctly (screenshots)
2. All props produce expected visual output
3. All events fire correctly (state feedback)
4. Feature integrates with other widgets
5. No console errors or warnings
6. No backend errors or exceptions

**Code patterns (refer to PlaywrightKnowledge-Index.md for specific files):**

- Use `getByText()`, `getByRole()` locators
- Use `.first()` when multiple matches possible
- Use `waitForTimeout(500)` after interactions
- On Windows use `shell: true` in spawn options
- Resolve project root: `process.cwd().replace(/[/\\]\.ivy[/\\]tests$/, "")`
- Wait for server ready by polling HTTP, not just stdout
- Use `takeScreenshotIfNotEmpty()` instead of raw `page.screenshot()` — skips blank pages

**Process management pattern:**

```typescript
import { test, expect } from '@playwright/test';
import { spawn, ChildProcess } from 'child_process';
import http from 'http';
import net from 'net';
import path from 'path';
import { trackProcess, killAllTrackedProcesses } from './test-utils';

test.describe('Feature Tests', () => {
  let serverProcess: ChildProcess;
  let port: number;
  const projectRoot = process.cwd().replace(/[/\\]\.ivy[/\\]tests$/, '');

  test.setTimeout(60000); // 60s timeout per test

  test.beforeAll(async () => {
    // Find free port
    port = await new Promise<number>((resolve) => {
      const server = net.createServer();
      server.listen(0, () => {
        const addr = server.address();
        const port = typeof addr === 'string' ? 0 : addr?.port ?? 0;
        server.close(() => resolve(port));
      });
    });

    // Spawn dotnet process
    serverProcess = spawn(
      'dotnet',
      ['run', '--no-build', '--', '--port', port.toString()],
      {
        cwd: projectRoot,
        shell: true,
        stdio: ['ignore', 'pipe', 'pipe'],
      }
    );

    // Track for cleanup on timeout/crash
    trackProcess(serverProcess);

    // Wait for server ready
    await waitForServer(`http://localhost:${port}`, 30000);
  });

  test.afterAll(() => {
    // Kill this process and any other tracked processes
    killAllTrackedProcesses();
  });

  // ... tests ...
});
```

### 8. Install & Run Tests

```bash
cd <ArtifactsDir>/sample/.ivy/tests
npm install
npx playwright install chromium
npx playwright test
```

### 8.5. Post-Test Cleanup

Even if tests pass, kill this plan's sample processes to ensure clean state:

```bash
powershell.exe -NoProfile -Command "Get-Process -ErrorAction SilentlyContinue | Where-Object { \$_.Path -and \$_.Path -like '<ArtifactsDir>*' } | Stop-Process -Force -ErrorAction SilentlyContinue"
```

### 9. Fix Loop (up to 10 rounds)

If tests fail, logs have errors, or screenshots show issues:

1. Analyze failures — categorize as:
   - **Test code issue** → fix `.spec.ts`
   - **Demo app issue** → fix `.cs` files
   - **Framework bug** → document in report
2. Apply fixes and re-run
3. Track each fix round

### 10. Verify Artifacts

Everything is already in place under `<ArtifactsDir>/`:

- `sample/` — `.csproj`, `.cs` files, `.ivy/tests/` (runnable project)
- `screenshots/` — Playwright screenshots
- `tests/` — `console.log`, `backend.log`

Confirm all expected files exist before writing the report.

### 11. Write Verification Report

Write to `<VerificationDir>/IvyFrameworkVerification.md`:

```markdown
# IvyFrameworkVerification

- **Plan:** <planId> — <title>
- **Date:** <CurrentTime>
- **Result:** Pass / Fail
- **Test Project:** <path to temp project>

## What was tested

<description of what was verified>

## Completeness

| Artifact | Status | Path |
|----------|--------|------|
| Sample App | Found/Missing/N/A | path or N/A |
| Documentation | Found/Missing/N/A | path or N/A |

## Props Tested

| Prop | Status | Notes |
|------|--------|-------|

## Events Tested

| Event | Status | Notes |
|-------|--------|-------|

## Visual Quality

<assessment of appearance and UX>

## Log Cleanliness

### Frontend Console
<clean / issues found>

### Backend Logs
<clean / issues found>

## Artifacts

- Screenshots: <list>
- Sample app: <path>

## Issues Found

| Issue | Severity | Area | Details |
|-------|----------|------|---------|

## Recommendations

<any suggestions>
```

### Rules

- Do NOT modify any source code in the Ivy Framework repos — this is a verification step only
- If verification fails, describe the failure clearly in the report
- Always produce a report, even for non-visual changes (just note it was skipped)
- Always read Memory files before creating test code — they contain critical gotchas
- Screenshots are evidence — take many, with descriptive names
- Keep demo apps focused — each tests a specific aspect
