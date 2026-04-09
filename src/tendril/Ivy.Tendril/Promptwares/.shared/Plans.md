# Plans File Structure

Plans are stored in the folder specified by `planFolder` in `config.yaml`.

## Directory Layout

```
{planFolder}/
├── .counter                          # Next plan ID (integer, auto-incremented)
├── 01098-MakeAnEmptyAppCalledReview/
│   ├── plan.yaml                     # Plan metadata
│   ├── revisions/                    # Plan content versions
│   │   ├── 001.md                    # Initial revision (created by MakePlan)
│   │   ├── 002.md                    # After ExpandPlan/UpdatePlan/SplitPlan
│   │   └── ...
│   ├── logs/                         # Execution logs per promptware run
│   │   ├── 001-MakePlan.md
│   │   ├── 002-ExpandPlan.md
│   │   └── ...
│   ├── artifacts/                    # Output artifacts from execution
│   │   ├── tests/                    # Test scripts and data
│   │   ├── screenshots/              # UI screenshots
│   │   └── sample/                   # Sample apps exercising new functionality
│   ├── verification/                 # Verification reports
│   │   ├── DotnetBuild.md
│   │   ├── DotnetTest.md
│   │   └── ...
│   ├── worktrees/                    # Git worktrees used during execution
│   ├── temp/                         # Scratch space for promptware agents
└── ...
```

## Folder Naming

`{ID:D5}-{SafeTitle}` — e.g. `01098-MakeAnEmptyAppCalledReview`

- **ID**: 5-digit zero-padded integer from `.counter`
- **SafeTitle**: Title-cased, first 60 chars of description, non-alphanumeric characters stripped, spaces removed (e.g. "Fix login bug" → `FixLoginBug`)

## plan.yaml

```yaml
state: Draft
project: Tendril
level: NiceToHave
title: "Make an empty app called Review"
sessionId: "a1b2c3d4-e5f6-..."
repos: []
created: 2026-03-28T20:36:39Z
updated: 2026-03-28T20:36:39Z
initialPrompt: "Make an empty app called Review"
sourceUrl: "https://github.com/owner/repo/issues/42"
prs: []
commits: []
verifications:
  - name: DotnetBuild
    status: Pending
  - name: DotnetTest
    status: Pending
relatedPlans: []
dependsOn: []
```

### Fields

| Field          | Description                                      |
|----------------|--------------------------------------------------|
| `state`        | Current plan state (see lifecycle below)         |
| `project`      | Project name matching a `projects` entry in `config.yaml` |
| `level`        | One of the levels defined in `config.yaml`       |
| `title`        | Human-readable plan title                        |
| `sessionId`    | Claude session ID from MakePlan (for `claude --resume`) |
| `repos`        | Affected repository paths (plain strings, e.g. `- D:\Repos\Foo` on Windows or `- /home/user/repos/Foo` on Linux — NOT objects) |
| `created`      | UTC timestamp when the plan was created (use `CurrentTime` from firmware header) |
| `updated`      | UTC timestamp of last state change (use `CurrentTime` from firmware header)      |
| `initialPrompt`| Original user description                        |
| `prs`          | Associated pull request URLs                     |
| `commits`      | Associated commit hashes                         |
| `verifications`| List of `{name, status}` — status is `Pending`, `Pass`, or `Fail` |
| `sourceUrl`    | (Optional) GitHub PR or issue URL that triggered this plan |
| `sourcePath`   | (Optional) Absolute path to the source that generated this plan (e.g. test working directory) |
| `relatedPlans` | Paths to related plan folders (parent plans, split-from, follow-ups) |
| `dependsOn`    | Plan folder names this plan depends on (e.g. `- 01478-WorktreeIsolation`). ExecutePlan will block until all dependencies are `Completed` and their PRs are merged. |

## State Lifecycle

```
MakePlan ──► Draft
               │
               ├─ ExpandPlan ──► Building ──► Draft
               ├─ UpdatePlan ──► Updating ──► Draft
               ├─ SplitPlan  ──► Updating ──► Skipped
               │
               ├─ ExecutePlan (dependencies unmet)
               │    Draft ──► Blocked ──► Draft (when unblocked) ──► Building ──► ...
               │
               ├─ ExecutePlan (Execute button)
               │    Draft ──► Building ──► Executing ──► ReadyForReview
               │                                    └──► Failed
               │
               ├─ MakePr (from Review app)
               │    ReadyForReview ──► Completed
               │
               ├─ (manual) ──► Skipped
               └─ (manual) ──► Icebox
```

| State            | Meaning                                    | Visible in      |
|------------------|--------------------------------------------|-----------------|
| `Draft`          | Ready for review/action                    | Plans           |
| `Building`       | ExpandPlan or ExecutePlan in progress       | Jobs            |
| `Updating`       | UpdatePlan or SplitPlan in progress         | Jobs            |
| `Executing`      | ExecutePlan agent running                   | Jobs            |
| `ReadyForReview` | ExecutePlan finished, awaiting human review | Review          |
| `Failed`         | ExecutePlan errored                         | Review          |
| `Completed`      | PR created, plan done                       | —               |
| `Skipped`        | Manually dismissed or split                 | —               |
| `Blocked`        | Waiting for dependency plans to complete     | Plans           |
| `Icebox`         | Parked for later                            | Icebox          |

## Revisions

Markdown files in `revisions/` numbered sequentially (`001.md`, `002.md`, ...).

The initial revision is created by MakePlan using the `planTemplate` from `config.yaml`.

Subsequent revisions are written by ExpandPlan, UpdatePlan, or SplitPlan agents.

## Logs

Each promptware run writes a log entry to `logs/` as `{NNN}-{Action}.md`:

```markdown
# MakePlan

- **Completed:** 2026-03-28T20:36:41Z
- **Status:** Completed
```

## temp/

Scratch space for promptware agents. Use this for any intermediate data needed during execution:
- Cloned external repos
- Downloaded files
- Generated intermediary files (e.g. pandoc input for PDF generation)
- Any temporary data the agent needs during the session

Contents are not tracked or committed. Can be safely deleted after the plan is completed.

## .counter

A single integer in `{planFolder}/.counter`. Read and incremented by the MakePlan agent when creating a new plan.

## Verifications

Each plan revision has a `## Verification` section with a checklist of verification steps from `config.yaml`:

```markdown
## Verification

- [x] DotnetBuild
- [x] DotnetTest
- [ ] FrontendLint
```

- `- [x]` = will be run by ExecutePlan after committing
- `- [ ]` = skipped

Required verifications are pre-checked by MakePlan based on the project's config. The user can edit the checklist before execution.

Verification definitions (name + prompt) live in the top-level `verifications` section of `config.yaml`. Projects reference them by name with a `required` flag.

## Notes

- **When referencing local files, folders, or screenshots in plans, use markdown links with the filename as display text: `[Button.cs](file:///path/to/repo/src/components/Button.cs)`. This allows the user to open files directly in VS Code by clicking the link while keeping plans readable.**
- Use markdown format for images ![alt text](image-url) for images.
- **Diagrams**: Our Markdown renderer supports Graphviz/DOT (```dot or ```graphviz code blocks) and Mermaid (```mermaid code blocks). **Prefer Graphviz/DOT over Mermaid** — it produces cleaner, more predictable layouts. Use diagrams sparingly — only when a visual genuinely clarifies architecture, data flow, or state transitions. Most plans don't need diagrams.

