# Ivy Tendril — Agent Instructions

## Plan Schema Migrations

When changing the `plan.yaml` structure (adding/removing/renaming fields, changing field types):

1. **Update `Plans.md`** (`Promptwares/.shared/Plans.md`) — this is the source of truth for the plan schema
2. **Add a repair step** in `PlanReaderService.RepairPlans()` — this runs on every Tendril startup and must migrate all existing plans to the new format
3. **Keep `PlanYaml.cs` in sync** — the deserialization model must match what `Plans.md` documents
4. **Update promptware instructions** — any promptware that writes `plan.yaml` (MakePlan, ExecutePlan, UpdatePlan, SplitPlan, ExpandPlan) must produce the new format

Existing plans on disk are never recreated — they must be repaired in place. If `RepairPlans()` can't fix a plan, it will silently fail and that plan won't appear in the UI. Always test your repair logic against real plan files.

## Project Structure

- `Services/` — ConfigService, PlanReaderService, JobService, GitService
- `Apps/` — PlansApp, ReviewApp, JobsApp, IceboxApp, and their views
- `Promptwares/` — MakePlan, ExecutePlan, UpdatePlan, SplitPlan, ExpandPlan, MakePr, IvyFrameworkVerification
- `Promptwares/.shared/` — Shared utilities (Utils.ps1, Plans.md, Firmware.md)
- `AppShell/` — Custom TendrilAppShell with sidebar badges

## Config

### Environment Variables

Tendril uses these environment variables:

- **`TENDRIL_HOME`** (required): Base path for all Tendril data (Plans/, Inbox/, Trash/, config.yaml, etc.)
  - Must be set before starting Tendril, otherwise onboarding is triggered
  - Example: `/home/user/.tendril` or `C:\Users\User\.tendril`

### Path Resolution

All paths derive from these sources:
1. `TENDRIL_HOME` environment variable (required) - points to config directory
2. Standard environment variables expanded via `%VAR%` syntax in config.yaml
3. Firmware header variables (`PlanFolder`, `PlansDirectory`, `ArtifactsDir`, etc.) derived from TENDRIL_HOME

**Never hardcode absolute paths** like `D:\Tendril` or `D:\Plans` in code or promptware instructions — always use the config values or firmware header variables.
