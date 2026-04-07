# UpdatePlan

Update an existing plan by applying user comments (lines prefixed with `>>`).

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
- The latest revision contains `>>` comment lines — these are user instructions

### 2. Parse Comments

Look for lines prefixed with `>>`. These are either:
- **Questions** (contain `?` or start with question words) — research and answer them
- **Instructions** — changes to incorporate into the plan

If no `>>` lines exist, report "No comments found" and stop.

### 3. Research and Answer Questions

For each question in the `>>` lines:
1. Read relevant source files to find the answer
2. Read `config.yaml` (from `TENDRIL_CONFIG` environment variable) for project context if needed

### 3.5. Resolve Answered Questions

Compare each existing question in `## Questions` against:
- The `>>` comments (user may have directly answered a question)
- Your research findings from step 3

For each question, determine if it has been answered — either explicitly by a `>>` comment or implicitly by a decision made in the updated plan. If answered:
- Wrap the question in a `<details>` block (collapsed) with the answer as the body
- The answer should reference the user's comment or the design decision that resolves it

If all questions are resolved and no new questions arose, omit the `## Questions` section entirely.

### 4. Apply Changes

- Create a new revision file (next sequential number, e.g. `002.md`)
- Incorporate the intent of each `>>` instruction into the updated plan
- Maintain the `## Questions` section (placed after the title, before `## Problem`) using `<details>` tags: (1) Existing questions answered by `>>` comments or research should be collapsed into `<details>` blocks with the answer. (2) New `>>` questions become new `<details>` blocks with answers. (3) Unanswered questions from prior revisions remain as open items (not in `<details>`). (4) If all questions are resolved and no new ones arose, omit the section entirely. Format:
  ```html
  <details>
  <summary>Question</summary>
  Answer
  </details>
  ```
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
