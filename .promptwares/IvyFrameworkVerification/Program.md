# IvyFrameworkVerification

Visually verify Ivy Framework UI changes by building and testing the samples app.

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

### 2. Build and Launch Samples App

Run IvyFeatureTester to build and verify the changes:

```powershell
cd D:\Repos\_Ivy
D:\Repos\_Personal\Scripts\AF2\IvyFeatureTester.ps1 "Plan <planId>: <description>. Test with <scenario>."
```

Replace `<planId>` with the plan ID from the folder name, `<description>` with the plan title, and `<scenario>` with a concrete test scenario based on the plan's Solution section.

### 3. Produce Artifacts

Save verification artifacts to the `ArtifactsDir`:

- **screenshots/** — screenshots of the UI showing the changes
- **sample/** — sample app code that exercises the new functionality (copy from Ivy.Samples.Shared if applicable)
- **tests/** — any visual test scripts or test data used
- **videos/** — screen recordings if applicable

### 4. Write Verification Report

Write a report to `<VerificationDir>/IvyFrameworkVerification.md`:

```markdown
# IvyFrameworkVerification

- **Plan:** <planId> — <title>
- **Date:** <CurrentTime>
- **Result:** Pass / Fail

## What was tested

<description of what was visually verified>

## Artifacts

- Screenshots: <list of screenshot files>
- Sample app: <path to sample app if created>

## Issues Found

<any issues discovered, or "None">
```

### Rules

- Do NOT modify any source code in the repos — this is a verification step only
- If verification fails, describe the failure clearly in the report
- Always produce a report, even for non-visual changes (just note it was skipped)
