---
searchHints:
  - overview
  - what-is
  - tendril
  - agent
  - orchestration
  - architecture
icon: Rocket
---

# Welcome to Tendril

<Ingress>
Tendril is an AI orchestration app on the Ivy stack: a cross-platform UI plus autonomous agents for real software workflows—not a black box.
</Ingress>

<Embed Url="https://youtu.be/Gkj5aj5nEKA"/>

<Callout type="tip">
You can always report issues and suggestions on the [GitHub repository](https://github.com/Ivy-Interactive/Ivy-Framework/issues).
If you need direct help, please join our [Discord](https://discord.gg/FHgxkDga3y).
</Callout>

You see each stage of the work. Tasks are **Plans**; orchestrated **Promptwares** (Claude-based agents) generate code, verify it, and open PRs—without hiding what ran.

## The Concept

**Plans** are structured units of work (bugfix, refactor, feature). Tendril moves them through a defined lifecycle using isolated, single-purpose agents called **Promptwares**.

## Key Features

- **Plan lifecycle** — Draft – execution – review – PR.
- **Multi-project** — Several repos, per-project verification rules.
- **Jobs** — Status, tokens, cost.
- **Promptwares** — e.g. `MakePlan`, `ExecutePlan`, `ExpandPlan`, `MakePr`.
- **Git worktrees** — Agent work stays off your main branch.
- **Terminal & file viewer** — Embedded terminal (Claude Code under the hood) and fast local file access.
- **Verification** — Hook your build, test, and format checks.

## The Tendril Loop

Tendril runs work as a **pipeline**, not a single chat reply. Each stage is a **job** (status, logs, tokens, cost) so you always know what ran. The usual path looks like this:

1. **`MakePlan`** — Turn a brief or issue into a structured plan.
2. **`ExpandPlan`** — Optionally break a large plan into smaller chunks.
3. **`ExecutePlan`** — Implement in a **git worktree**, run **verifications**, iterate until checks pass.
4. **`Review`** — You approve the result or send feedback for another pass.
5. **`MakePr`** — Open a **GitHub PR** from the approved work.

Below, each step is described the way you’ll use it in the UI—good anchors for screenshots.

### MakePlan

You start from a short description, a pasted issue, or content from **Drafts** / **Inbox**. **MakePlan** produces a real plan: problem, proposed solution, tests, and verification commands. Nothing executes yet—you get a draft you can read and edit before any code runs.

When you create a plan from the UI, **Create New Plan** walks through three fields (labels match the dialog):

![Create New Plan: project, priority, and task description](/tendril-docs/assets/create-new-plan.png "Create New Plan dialog")

- **Select project(s)** — Choose **Auto** or one or more **named projects**. Names come from the **projects** list in your Tendril config (`TENDRIL_HOME/config.yaml`; same entries as **Settings → Projects**). **Auto** and specific projects are mutually exclusive: selecting **Auto** clears named picks, and picking a named project clears **Auto**.
- **Priority** — **Normal**, **High**, or **Urgent** is stored on the plan and used when **ordering jobs** in the queue—if several plans are waiting to run, higher priority is handled first.
- **Describe the task for the new plan** — The large text area (“Enter task description…”) is where you describe the issue or work to do. **MakePlan** turns that text into the structured plan (problem, approach, tests).

### ExecutePlan

After you **Create** a new plan, work is scheduled as **jobs** you follow under **Jobs** in the sidebar: one row per promptware run (**MakePlan** first, then **ExecutePlan**, **MakePr**, and others as the plan moves forward). The list is the live view of every plan’s execution—not a hidden background task.

![Jobs: executions with status, columns, and row actions](/tendril-docs/assets/jobs-dashboard.png "Jobs dashboard")

For each job you typically see **status** (e.g. Running, Completed), the **plan** id (link), **prompt/title**, **type** (`MakePlan`, `ExecutePlan`, `MakePr`, …), **project**, **timer** and **last output** timing, **cost**, **tokens**, and a concise **status** message for what the agent is doing. A header summary shows aggregate progress (e.g. how many completed vs still running).

Row actions (icons on the right) let you **open the associated plan**, **view job output** (including **JSON** where applicable), **show the full prompt text**, **stop** a running job, **refresh** the list, and **move the job to trash**. See [Jobs](../04_Apps/04_Jobs.md) for more.

**ExecutePlan** is where code changes happen. The agent works in an isolated **worktree** (not your main checkout), runs your **build / format / test** verification steps, and retries when something fails. Logs and command output stream into the job so failures are visible—not summarized away.

### Drafts

**Drafts** is the screen for plans still in **Draft** (or **Blocked**): you read the latest revision (problem, solution, tests) on tabs like **Plan**, **Summary**, and **Verifications**, and you can keep it open while jobs run—watch **Jobs** for live output and return here as the revision and status update.

![Drafts: plan view with Execute and toolbar actions](/tendril-docs/assets/drafts-plan-view.png "Drafts — plan view")

Common actions on the plan:

- **Execute** — Start **ExecutePlan** against the current solution (creates a new job that implements the plan in a worktree).
- **Update** — Open a dialog to describe what should change; Tendril runs **UpdatePlan** so the draft reflects your feedback.
- **Split** — Run **SplitPlan** to break the work into separate plans.
- **Expand** — Run **ExpandPlan** to add detail when the plan is too thin.
- **Delete** — Remove the draft plan when you no longer need it.

See [Drafts](../04_Apps/03_Drafts.md) for the full picture.

### Review

Open the **Review** app to work through plans in **Ready for review** (and **Failed**, so you can inspect or rerun). The sidebar lists plans; use search and filters to narrow the list, then select a plan to load it in the main panel.

![Review app: plan list, Verifications tab with pass/skip status, Make PR, and footer actions](/tendril-docs/assets/review-app.png "Review — sidebar, Verifications tab, Make PR, and toolbar")

Use the **tabs** to inspect the run:

- **Summary** — Agent-written summary from artifacts, when present.
- **Verifications** — Status per check; **click a verification name** when a report exists to read it in a side sheet.
- **Git** — Commits and linked **pull requests** (open a commit for message and diff; open PR URLs in the browser).
- **Changes** — File list and **diff** across the plan’s commits.
- **Artifacts** — Screenshots and other outputs, when present.
- **Recommendations** — Structured follow-ups from `recommendations.yaml`, when present.
- **Plan** — Latest plan revision markdown (problem, solution, tests).

The **toolbar** at the bottom drives what happens next:

- **Rerun** (**R**) — Run execution again (pick scope in the dialog).
- **Suggest changes** (**D**) — Send feedback so the agent can revise the plan or code in another pass.
- **Discard** — Move the plan to trash if you are abandoning it.
- **Previous** (**P**) / **Next** (**N**) — Move between plans in the current list (the header also shows **n / total** plans).

The header has **Make PR** (**M**), which starts the PR flow. The **⋯** (overflow) menu adds **Custom PR**, **Set completed**, and shortcuts to open the plan folder, terminal, editor, or `plan.yaml`.

<Callout type="warning">
If every configured repository for the plan uses the **yolo** PR rule, **Make PR** starts the **MakePr** job immediately—fully automated, and the result may **merge** per your repo and GitHub settings. To open a PR through the **Custom PR** dialog first (assignee, comment, merge options, and a normal review cycle), use **⋯ Custom PR** instead of **Make PR**. When any repo is **not** **yolo**, **Make PR** already opens that dialog.
</Callout>

### MakePr

**MakePr** turns the plan’s worktree changes into a **GitHub pull request** (via `gh` against your configured repo). The PR stays tied to the same plan and jobs that produced the branch.

When you use the **Custom PR** dialog (**Make PR** when not all repos are **yolo**, or **⋯ → Custom PR**), you can tune how the PR is opened:

- **Merge** — When enabled, the automation requests **auto-merge** for the PR (subject to GitHub branch protections and repo settings).
- **Delete branch** — When enabled (and **Merge** is on), the source branch can be removed after the PR merges. It is disabled until **Merge** is checked.
- **Include artifacts** — Attach plan artifacts to the PR flow when your setup supports it.
- **Assignee** — Optional GitHub assignee (from the repo’s collaborators when available).
- **Comment** — This text is sent as the **PR body / description** on GitHub, not an internal note.

![Custom PR dialog: Merge, Delete branch, Include artifacts, Assignee, and Comment](/tendril-docs/assets/custom-pr-dialog.png "Custom PR — merge options, assignee, and description")

That loop turns the assistant from autocomplete into something you can ship with.

### What stays on disk (after the loop)

When you complete **onboarding**, you choose a directory for Tendril data—that directory becomes **`TENDRIL_HOME`** (also set in the environment so tools and promptware can find it). Everything Tendril persists on disk lives under that folder: **`config.yaml`**, **Plans**, **Inbox**, **Trash**, **Promptwares**, **Hooks**, and other top-level files (for example crash logs). It is the single root for your workspace data, not scattered across the repo.

Each plan is a **folder under** `TENDRIL_HOME/Plans/` named like `00042-ShortTitle`. Inside you get metadata (`plan.yaml`), **revisions**, **verification** output, **logs** (Markdown files per job under `logs/`, e.g. `001-ExecutePlan.md`), **costs** (`costs.csv`), and **worktrees**. You can diff, grep, or back up plans like normal files; the UI is a lens on that data.

![Finder column view: TENDRIL_HOME, Plans, a plan folder, and logs with ExecutePlan markdown files](/tendril-docs/assets/tendril-home-plans-logs.png "Tendril data directory — plan folder and logs on disk")

For the full layout and states, see [Plans](../02_Concepts/01_Plans.md) and [Lifecycle & Jobs](../02_Concepts/03_Lifecycle.md).
