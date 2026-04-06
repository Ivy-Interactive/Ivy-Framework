# ExpandPlan

Transform investigation-heavy plans into concrete implementation plans.

## Context

The firmware header contains:
- **Args** / **PlanFolder** — path to the plan folder
- **CurrentTime** — current UTC timestamp

Read the plan structure in `../.shared/Plans.md`.
Read `config.yaml` from the `TENDRIL_CONFIG` environment variable (absolute path to config.yaml).

## Execution Steps

### 1. Read the Plan

- Read `plan.yaml` from the plan folder
- Read the latest revision from `revisions/` (highest numbered .md file)
- Identify sections with investigative/exploratory language ("Investigate...", "Check if...", "Research...", "Explore...")

### 2. Research and Resolve

For each investigation section:

1. **Read relevant source files** to understand the current implementation
2. **Answer the investigation questions** by examining code, docs, and patterns
3. **Transform into concrete steps** — replace "Investigate X" with specific implementation tasks

Example:

**Before:**
```
1. Investigate the dialog rendering lifecycle:
   - Check dialog component
   - Check form builder
```

**After:**
```
1. Fix dialog content initialization race condition:
   - In `Dialog.cs`, add immediate content rendering before animation
   - In `FormBuilder.cs`, ensure UseState hooks execute synchronously in dialog context
```

### 3. Create Expanded Revision

- Create a new revision file (next sequential number, e.g. `002.md`)
- Replace all investigative/exploratory language with specific actions
- Include exact file paths for changes
- Specify concrete code modifications or additions
- Maintain all original context and problem description
- Preserve the plan template structure (from `planTemplate` in config.yaml)

### Rules

- If the expanded revision has no questions, omit the `## Questions` section entirely
- The expanded plan must be **immediately actionable** without further investigation
- If research reveals the problem is already solved or doesn't exist, note that clearly
- Do NOT modify the original revision — always create a new revision file
- Do NOT modify any source code — only read files and update the plan
- Do NOT modify `plan.yaml` — the launcher script handles state and timestamps
- Keep the plan short and concise — the limiting factor is a human reading it
- When referencing local files, use markdown links: `[FileName.cs](file:///path/to/FileName.cs)` (and ![...](...) for images)
