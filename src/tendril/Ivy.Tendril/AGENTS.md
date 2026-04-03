# Ivy Tendril — Agent Instructions

## Plan Schema Migrations

When changing the `plan.yaml` structure (adding/removing/renaming fields, changing field types):

1. **Update `Plans.md`** (`.promptwares/.shared/Plans.md`) — this is the source of truth for the plan schema
2. **Add a repair step** in `PlanReaderService.RepairPlans()` — this runs on every Tendril startup and must migrate all existing plans to the new format
3. **Keep `PlanYaml.cs` in sync** — the deserialization model must match what `Plans.md` documents
4. **Update promptware instructions** — any promptware that writes `plan.yaml` (MakePlan, ExecutePlan, UpdatePlan, SplitPlan, ExpandPlan) must produce the new format

Existing plans on disk are never recreated — they must be repaired in place. If `RepairPlans()` can't fix a plan, it will silently fail and that plan won't appear in the UI. Always test your repair logic against real plan files.

## Project Structure

- `Services/` — ConfigService, PlanReaderService, JobService, GitService
- `Apps/` — PlansApp, ReviewApp, JobsApp, IceboxApp, and their views
- `.promptwares/` — MakePlan, ExecutePlan, UpdatePlan, SplitPlan, ExpandPlan, MakePr, IvyFrameworkVerification
- `.promptwares/.shared/` — Shared utilities (Utils.ps1, Plans.md, Firmware.md)
- `AppShell/` — Custom TendrilAppShell with sidebar badges

## Config

### Environment Variables

Tendril uses these environment variables:

- **`TENDRIL_HOME`** (required): Base path for all Tendril data (Plans/, Inbox/, Trash/, etc.)
  - Overrides `tendrilData` in config.yaml if set
  - Example: `/home/user/.tendril` or `C:\Users\User\.tendril`

- **`REPOS_HOME`** (optional): Base path where repositories are located
  - Overrides `reposHome` in config.yaml if set
  - Example: `/home/user/repos` or `C:\Users\User\repos`
  - **Optional convenience** - you can hardcode individual repo paths in config.yaml instead

### Path Resolution

All paths derive from these sources (in order of precedence):
1. Environment variables (`TENDRIL_HOME`, `REPOS_HOME`)
2. config.yaml (`tendrilData`, `reposHome`)
3. Firmware header variables (`PlanFolder`, `PlansDirectory`, `ArtifactsDir`, etc.)

**Never hardcode absolute paths** like `D:\Tendril` or `D:\Plans` in code or promptware instructions — always use the config values or firmware header variables.
