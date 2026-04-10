# Caller/Consumer Audit Pattern

**Created**: 2026-04-06 (Session 00781)
**Updated**: 2026-04-07 (Session 00982) — Generalized from interface-only to all rename/refactor plans

## The Problem

When creating plans that rename or refactor symbols, manually enumerating callers is error-prone. Planners tend to search only the obvious directory and miss callers in adjacent directories or projects.

## Incidents

### Plan 01927 (Interface Extraction)

Initial revision identified 1 consumer in `Review/ContentView.cs`. Execution found 2 additional consumers: `ReviewApp.cs` and `CreateIssueDialog.cs`.

**Root cause**: Only checked the obvious file, missed related files using the same DI patterns.

### Plan 02123 (Function Rename)

Initial revision listed 7 callers of `ReadPlanProject` in `.promptwares/`. Execution found an 8th caller in `Ivy.Tendril.TeamIvyConfig/Promptwares/IvyFrameworkVerification.ps1` — a different directory entirely.

**Root cause**: Grep was scoped to `.promptwares/` instead of the full repo root. The planner assumed all callers were in the same directory tree.

## The Pattern: Exhaustive Caller Audit

When any plan involves renaming, signature changes, interface extraction, or type refactoring:

### Step 1: Search the full repo

Always grep from the **repo root**, not the expected subdirectory:

```
Grep pattern="FunctionName" path="<repo-root>" output_mode="content"
```

### Step 2: List all callers with line numbers

For each match, record file path and line number. Read context to confirm it's a real caller (not a comment or string).

### Step 3: Validate completeness

Count grep matches vs documented callers. If mismatch, investigate.

### When to Apply

- Function/method renames
- Interface extraction from concrete types
- Service replacement/refactoring
- DI registration changes
- Type renames that affect consumers

### DI-Specific Patterns (for interface extraction)

Also search these patterns:
- `UseService<ConcreteType>()`
- Constructor parameter injection: `ConcreteType paramName`
- Field/property declarations: `_concreteType` or `concreteType:`

## Related Plans

- **01927** — ExtractGithubServiceAndGitServiceInterfaces (missed 2 consumers)
- **01943** — FixRemainingServiceInterfaceUsage (follow-up)
- **02123** — AuditAndRenameUtilsPs1FunctionNames (missed 1 caller in different directory)
