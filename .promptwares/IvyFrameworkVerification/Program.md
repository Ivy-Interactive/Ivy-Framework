# IvyFrameworkVerification

Test and visually verify Ivy Framework UI changes by creating demo apps and running Playwright tests.

## Context

The firmware header contains:
- **PlanFolder** — path to the plan folder
- **ConfigPath** — absolute path to config.yaml
- **CurrentTime** — current UTC timestamp
- **VerificationDir** — path to write the verification report
- **ArtifactsDir** — path to store test artifacts (screenshots, videos, sample apps)

## Execution Steps

### 1. Read Plan

- Read `plan.yaml` from the plan folder
- Read the latest revision from `revisions/` to understand what changed
- Determine if the changes affect visual/UI behavior

If the changes are non-visual (docs, analyzers, refactoring, code-only fixes), write a report noting "No visual verification needed" and exit successfully.

### 2. Research

- Read `Memory/IvyFrameworkGotchas.md` for known API issues and workarounds
- Read `Memory/PlaywrightKnowledge.md` for Ivy testing patterns and locator strategies
- Read the Ivy Framework AGENTS.md: `D:\Repos\_Ivy\Ivy-Framework\AGENTS.md`
- Read relevant source code for the changed feature from `D:\Repos\_Ivy\Ivy-Framework\src\`
- Read existing samples: `D:\Repos\_Ivy\Ivy-Framework\src\Ivy.Samples.Shared\Apps\`

### 3. Verify Completeness (Widgets Only)

If the feature is a **widget**, check that required companion artifacts exist:

1. **Sample App**: Search `D:\Repos\_Ivy\Ivy-Framework\src\Ivy.Samples.Shared\Apps\` for files containing the widget name
2. **Documentation Page**: Search `D:\Repos\_Ivy\Ivy-Framework\src\Ivy.Docs.Shared\Docs\02_Widgets\` for a matching `.md` file

Record results for the report. Skip for non-widget features.

### 4. Create Temp Project

Create folder: `D:\Temp\IvyVerification\<yyyy-MM-dd>\<FeatureName>\`

**`<FeatureName>.csproj`:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="D:\Repos\_Ivy\Ivy-Framework\src\Ivy\Ivy.csproj" />
    <ProjectReference Include="D:\Repos\_Ivy\Ivy-Framework\src\Ivy.Analyser\Ivy.Analyser.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
  </ItemGroup>
</Project>
```

**`Program.cs`:**
```csharp
using Ivy;
using System.Reflection;

var server = new Server();
server.AddAppsFromAssembly(Assembly.GetExecutingAssembly());
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

```bash
dotnet build
dotnet run --describe
```

Fix any compilation errors. Iterate until build succeeds.

### 7. Create Playwright Tests

Create `.ivy/tests/` directory with:

**package.json** — minimal, with `@playwright/test` dependency

**playwright.config.ts** — Chromium only, single worker, no retries, viewport `{ width: 1920, height: 1920 }` (set in both `use` and `projects[0].use`), uses `process.env.APP_PORT`, video recording: `video: { mode: 'on', dir: './videos' }` in both `use` and `projects[0].use`

**One `.spec.ts` per app:**
- `beforeAll`: find free port, spawn `dotnet run -- --port <port>`, wait for HTTP 200
- `afterAll`: kill process
- Test each app at `http://localhost:<port>/<app-id>?shell=false`
- Take screenshots at every key step
- Use global screenshot counter with descriptive names
- Capture browser console logs → `.ivy/tests/console.log`
- Capture backend stdout/stderr → `.ivy/tests/backend.log`

**Videos:**
- Playwright records video per test via config
- After each test, save video with descriptive name:
  ```typescript
  test.afterEach(async ({ page }, testInfo) => {
    const video = page.video();
    if (video) {
      const videoPath = await video.path();
      const targetName = testInfo.title.replace(/[^a-zA-Z0-9]/g, '-').toLowerCase();
      const targetPath = path.join(__dirname, 'videos', `${targetName}.webm`);
      await fs.promises.mkdir(path.dirname(targetPath), { recursive: true });
      await fs.promises.copyFile(videoPath, targetPath);
    }
  });
  ```

**Test coverage must verify:**
1. Feature renders correctly (screenshots)
2. All props produce expected visual output
3. All events fire correctly (state feedback)
4. Feature integrates with other widgets
5. No console errors or warnings
6. No backend errors or exceptions
7. Video captures show smooth interactions

**Code patterns (from PlaywrightKnowledge.md):**
- Use `getByText()`, `getByRole()` locators
- Use `.first()` when multiple matches possible
- Use `waitForTimeout(500)` after interactions
- On Windows use `shell: true` in spawn options
- Resolve project root: `process.cwd().replace(/[/\\]\.ivy[/\\]tests$/, "")`
- Wait for server ready by polling HTTP, not just stdout

### 8. Install & Run Tests

```bash
cd .ivy/tests
vp install
npx playwright install chromium
vp run test
```

### 9. Fix Loop (up to 10 rounds)

If tests fail, logs have errors, or screenshots show issues:

1. Analyze failures — categorize as:
   - **Test code issue** → fix `.spec.ts`
   - **Demo app issue** → fix `.cs` files
   - **Framework bug** → document in report
2. Apply fixes and re-run
3. Track each fix round

### 10. Copy Artifacts to Plan

Copy all evidence to the plan's artifacts directory (`ArtifactsDir`):

- `screenshots/` — all screenshots from `.ivy/tests/screenshots/`
- `videos/` — all videos from `.ivy/tests/videos/`
- `sample/` — the demo app `.cs` files
- `tests/` — the test `.spec.ts` files, console.log, backend.log

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

## Completeness (Widgets Only)

| Artifact | Status | Path |
|----------|--------|------|
| Sample App | Found/Missing | path or N/A |
| Documentation | Found/Missing | path or N/A |

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
- Videos: <list>
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
