# SplitPlan

Split a multi-issue plan into separate, self-contained plans.

## Context

The firmware header contains:
- **Args** / **PlanFolder** — path to the plan folder to split
- **CurrentTime** — current UTC timestamp

Read the plan structure in `../.shared/Plans.md`.
Use the `Get-ConfigYaml` helper from Utils.ps1 to read project configuration (available projects and their repos) with caching.

The plans directory path can be derived from the plan folder's parent directory.

## Execution Steps

### 1. Read the Plan

- Read `plan.yaml` from the plan folder
- Read the latest revision from `revisions/` (highest numbered .md file)
- Identify distinct issues/tasks that should be separate plans

### 2. Allocate Plan IDs

- Read the counter from `.counter` in the plans directory
- Reserve one ID per new plan and increment the counter atomically (read → reserve all needed IDs at once → write new value)
- Format as 5-digit zero-padded (e.g. `01205`)
- **Important:** Read and write the counter in a single operation to avoid race conditions with concurrent runs

### 3. Create Split Plans

For each distinct issue, create a new plan folder following the structure in `../.shared/Plans.md`:
- Create folder `{ID:D5}-{SafeTitle}/` (title-cased, no spaces)
- Create `plan.yaml` with appropriate project, level, title, and `CurrentTime` timestamps
- Create `revisions/001.md` using the `planTemplate` from `config.yaml`
- Fill in Problem, Solution, Remaining Design Questions, Tests sections
- Each plan must be fully self-contained

#### 3.1 Project Assignment

Each new plan may belong to a different project than the original. For each split plan:
- Analyze which project(s) from `config.yaml` are relevant based on the files/repos involved
- Set `project` in `plan.yaml` to the matching project name
- Set `repos` from that project's repo list in `config.yaml`
- Populate `verifications` from that project's verification list (all set to `Pending`)
- Generate the `## Verification` checklist using that project's required/optional verifications

If a sub-plan spans multiple projects, prefer the primary project (where most changes occur).

### 4. Original Plan

Do NOT modify the original plan's `plan.yaml` — the launcher script handles state and timestamps.

### Rules

- **Must produce at least 2 new plan folders** — if content can't be meaningfully split, report this and stop
- ONE issue per plan
- Each plan must include all paths and info for an LLM coding agent to execute end-to-end
- Keep each plan short and concise — the limiting factor is a human reading it
- Do NOT modify any source code — only read files and create plan folders
- When referencing local files, use markdown links: `[FileName.cs](file:///path/to/FileName.cs)` (and ![...](...) for images)
