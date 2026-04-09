# IvyFrameworkVerification .csproj Naming Issue

## Problem

When IvyFrameworkVerification was instructed to create `<FeatureName>.csproj`, plan reruns with "Suggested Changes" could generate multiple .csproj files if the feature name varied between revisions.

**Example from plan 02036:**
- First run: Created `GitHubCalloutSupport.csproj`
- Rerun with changes: Created `GitHubCalloutTest.csproj`
- Result: Both .csproj files exist, causing confusion and duplicate build artifacts

## Solution

Changed IvyFrameworkVerification Program.md (Step 4) to always use `Sample.csproj` as the consistent filename.

**Benefits:**
1. Reruns overwrite the same file instead of accumulating new ones
2. Clear, predictable build artifacts (Sample.dll, Sample.exe)
3. No confusion about which .csproj is the primary one
4. Prevents wasted resources building multiple projects

## Date

Fixed: 2026-04-06

## Related

- IvyFrameworkVerification Program.md: Step 4 "Create Sample Project"
- Example issue: Plan 02036-GitHubCalloutSupport
