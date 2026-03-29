# UpdatePlan

Update an existing plan by applying user comments (lines prefixed with `>>`).

## Context

The firmware header contains:
- **Args** / **PlanFolder** — path to the plan folder
- **ConfigPath** — absolute path to config.yaml
- **CurrentTime** — current UTC timestamp

Read the plan structure in `../.shared/Plans.md`.

## Execution Steps

### 1. Read the Plan

- Read `plan.yaml` from the plan folder
- Read the latest revision from `revisions/` (highest numbered .md file)
- The latest revision contains `>>` comment lines — these are user instructions

### 2. Parse Comments

Look for lines prefixed with `>>`. These are either:
- **Questions** (contain `?` or start with question words) — research and answer them
- **Instructions** — changes to incorporate into the plan

If no `>>` lines exist, report "No comments found" and stop.

### 3. Research and Answer Questions

For each question in the `>>` lines:
1. Read relevant source files to find the answer
2. Read `config.yaml` (from `ConfigPath` in header) for project context if needed

### 4. Apply Changes

- Create a new revision file (next sequential number, e.g. `002.md`)
- Incorporate the intent of each `>>` instruction into the updated plan
- Answer questions inline in a `## Questions` section with answers
- Remove all `>>` lines — they've been processed
- Preserve the plan template structure 
- The updated plan must be at least as comprehensive as the original

### Rules

- Do NOT modify any source code — only read files and update the plan
- Do NOT modify the original revision — always create a new revision file
- Do NOT modify `plan.yaml` — the launcher script handles state and timestamps
- The plan must remain self-contained with all paths and information for an LLM coding agent
- Keep the plan short and concise — the limiting factor is a human reading it
- When referencing local files, use markdown links: `[FileName.cs](file:///path/to/FileName.cs)` (and ![...](...) for images)
