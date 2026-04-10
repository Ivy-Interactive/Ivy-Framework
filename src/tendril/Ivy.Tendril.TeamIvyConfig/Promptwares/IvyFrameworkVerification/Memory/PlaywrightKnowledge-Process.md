# Process Management (Critical)

## Process Cleanup (Critical)

- **ALWAYS use `test-utils.ts` process tracking** — raw `afterAll` cleanup is insufficient for test timeouts and crashes
- Create `test-utils.ts` with `trackProcess()` and `killAllTrackedProcesses()` utilities (see IvyFrameworkVerification/Program.md Section 7)
- **Call `trackProcess(proc)`** immediately after spawning any dotnet process in `beforeAll`
- **Set explicit test timeout** with `test.setTimeout(60000)` (60s) to catch hung tests before Playwright's default 30s timeout
- **Use `killAllTrackedProcesses()`** in `afterAll` instead of manual `proc.kill()` — ensures all tracked processes are killed
- Process tracking registers global handlers for SIGINT, SIGTERM, uncaughtException, and exit events
- On Windows, uses `taskkill /F /T` for force-kill with child process termination
- This is **in addition to** the cleanup-on-next-run approach in IvyFrameworkVerification.ps1 (see plan 01834)

## App Lifecycle in Tests

- `beforeAll`: find free port via `net.createServer()`, spawn `dotnet run -- --port <port>`, wait for server ready
- **Health check must accept any HTTP status** — with `UseAppShell()`, the root URL returns HTTP 500 (no default app). Accept any status `< 600` to confirm server is up, not just 200
- `afterAll`: kill the spawned process
- `beforeEach`: navigate to `http://localhost:<port>`
- Use `cwd: process.cwd().replace(/[/\\]\.ivy[/\\]tests$/, "")` to resolve project root from test dir
- Wait for server with polling loop (500ms interval, 30s timeout)

## Stale Process Patterns

- **Stale server processes**: The `taskkill` in `afterAll` may not reliably kill all dotnet processes. After multiple test runs, stale `Test.*.exe` processes can lock the EXE and prevent rebuilding. The launcher script (`IvyFrameworkVerification.ps1`) now kills processes with executables in `artifacts/sample/bin/` before starting, but the `afterAll` kill should still be done as best-effort cleanup. Use `taskkill //im <name>.exe //f` as a fallback.
- Always kill app processes after test runs on Windows — lingering processes lock DLLs and prevent rebuild on next run. Use `powershell.exe -NoProfile -Command 'Get-Process -Name "AppName" -ErrorAction SilentlyContinue | Stop-Process -Force'`
- When stale processes lock DLLs and `dotnet run` fails repeatedly, spawn the pre-built exe directly: `spawn(path.join(projectRoot, "bin", "Debug", "net10.0", "AppName.exe"), ["--port", port], ...)`
- `powershell -Command "Get-Process -Name 'X' -ErrorAction SilentlyContinue | Stop-Process -Force"` is more reliable than `taskkill /f /im` for killing stale processes on Windows
- Multiple `test.describe` blocks in one file each trigger their own `beforeAll` — caused server re-spawn failure due to DLL locks from first server instance. Solution: use top-level `test()` (outside `describe`) for tests that need to share the same server lifecycle

## `beforeAll` Timeout for Recompilation

- When C# source is modified between test runs, `dotnet run` triggers a rebuild that can exceed the 30s default timeout. Always use `testInfo.setTimeout(120000)` in `beforeAll`

## `taskkill` in Bash on Windows

- `spawn('taskkill', ['/pid', ...], { shell: true })` fails with "Invalid argument/option" because bash interprets `/f` as a path. Use `spawn('cmd', ['/c', 'taskkill', '/pid', pid, '/f', '/t'], { shell: false })` instead
