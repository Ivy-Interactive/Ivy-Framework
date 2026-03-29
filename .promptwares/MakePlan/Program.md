# MakePlan

**⚠️ You may only write files inside the plans directory (see `PlansDirectory` in the firmware header). Do NOT create, edit, or delete any source code files.**

Create an implementation plan for a task described in args.

## Context

The firmware header contains these key values:
- **PlansDirectory** — where plan folders are created
- **ConfigPath** — absolute path to config.yaml (projects, repos, context)
- **Project** — selected project name, or `[Auto]` if not specified

Read the plan folder structure in `../.shared/Plans.md`.
Read the project configuration from the `ConfigPath` in the firmware header.

## Execution Steps

### 1. Parse Args

Args contains the user's task description. If it references related plans with `[number]` syntax (e.g. `[01205]`), find and read those plan files from `PlansDirectory` for context.

**Extract Criticality Level**: Look for a criticality or priority level indicator in Args.

### 1.5. Load Project Context

Read `config.yaml` (at the path from the firmware header) to understand all available projects, their repos, and context.

**If `Project` is set to a specific project name** (not `[Auto]` or `General`):
- Find that project in `config.yaml` and use its repos and context to scope your research

**If `Project: General`**:
- This is a project-agnostic plan (research, cross-cutting, ad-hoc tasks)
- Do not scope to any specific repos — research broadly as needed
- Set `project: General` in plan.yaml, leave `repos: []` empty

**If `Project: [Auto]`**:
- Analyze the task description to infer the correct project from `config.yaml`
- Match based on keywords, repo paths, or component names in the description
- If no project matches, use `General`
- Use the matched project's context to scope your research

### 2. Allocate Plan ID

- Read the counter from `PlansDirectory\.counter` (default 200 if missing)
- Reserve the next ID and increment the counter
- Format as 5-digit zero-padded (e.g. `01205`)

### 3. Research

- Read relevant source files to understand the codebase areas involved
- **Search GitHub issues** before creating plans to avoid duplicates or workaround plans for features already being built. Example:
  ```bash
  gh search issues "<keyword>" --repo <owner>/<repo> --json title,url,number,state
  ```
  Derive the repo owner/name from the repos in `config.yaml`. If an issue already covers the task, reference it in the plan and avoid creating workaround plans.

### 4. Create Plan

Create the plan folder, `plan.yaml`, and `revisions/001.md` according to the structure in `../.shared/Plans.md`.

In `plan.yaml`, populate the `verifications` list with each verification from the project's config, all set to `Pending`:

```yaml
verifications:
  - name: DotnetBuild
    status: Pending
  - name: DotnetTest
    status: Pending
```

If the plan references other plans (from `[number]` syntax in args), add them to `relatedPlans`.

### 5. Verification Checklist

In the `## Verification` section of the plan revision, generate a checklist from the project's `verifications` in `config.yaml`.

For each verification assigned to the project:
- **Required** (`required: true`) → `- [x] VerificationName`
- **Optional** (`required: false`) → `- [ ] VerificationName`

Example for a Framework project plan:
```markdown
## Verification

- [x] DotnetBuild
- [x] DotnetFormat
- [x] DotnetTest
- [x] FrontendLint
- [x] IvyFrameworkVerification
```

If the project has no verifications (e.g. `General`), leave the section empty or omit it.

The user can edit the checklist before execution — unchecking a required verification or checking an optional one. ExecutePlan will run only the checked items.

### Rules

- **!CRITICAL: Every MakePlan execution MUST produce at least one plan folder. Even if the task is an analysis, review, or investigation — always create a plan with actionable steps. Never just analyze and report back without a plan.**
- The plan must include all paths and information for an LLM coding agent to execute end-to-end without human intervention
- Keep the plan short and concise - the limiting factor of this system is a human that will have to read this.
- **!IMPORTANT: ONE issue per plan file — if multiple issues, create multiple plan files with separate IDs**