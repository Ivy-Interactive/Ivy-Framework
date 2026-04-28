# Converting Workflows to Skills: Lessons Learned

Notes from porting three Ivy Agent workflows (Dashboard, CRUD, AdHoc) from C# state machines into Claude Code skills, combined with official best practices from the [Agent Skills standard](https://agentskills.io/specification) and [Claude Code docs](https://code.claude.com/docs/en/skills.md).

---

## What Went Well

### The plan/review/implement loop translates cleanly
Each workflow had the same three-state core: generate a plan, let the user approve or revise, then implement. This mapped 1:1 to numbered steps in markdown. The state machine transitions (AgenticTransition, StateTransition, WorkflowTransition) became prose instructions -- simpler and easier to maintain than the C# class hierarchy.

### Reference files as concrete examples beat abstract rules
The workflows already embedded reference .cs files as learning material for the LLM. Keeping that pattern (a `references/` folder with real, compilable code) works better than describing patterns in prose. The LLM can read `OrderListBlade.cs` and generalize from it; a paragraph explaining "use UseQuery with tags" is less reliable.

### One source of truth via CopyReferences.ps1
The reference files are copies of canonical code in `Ivy.Internals.Workflows.References`. A copy script with namespace rewriting avoids drift between the actual framework examples and what the skills teach. Any future change to the reference code propagates with a single script run.

---

## What We Should Improve

### 1. Add YAML frontmatter

All three skills currently start with a plain `# heading`. The [Agent Skills spec](https://agentskills.io/specification) requires frontmatter with at least `name` and `description`. Claude Code uses these fields for skill discovery and activation.

**Current:**
```markdown
# ivy-create-dashboard

Create a Dashboard app with metrics, charts, and KPIs for an Ivy project.
```

**Should be:**
```yaml
---
name: ivy-create-dashboard
description: >
  Create a Dashboard app with metrics, charts, and KPIs for an Ivy project.
  Use when the user asks for a dashboard, analytics page, KPI view, or
  data visualization app. Handles metric cards, line/bar/pie/area charts,
  date range filtering, and grid layouts.
---
```

Key frontmatter fields to consider for each skill:
- `name` -- required, lowercase + hyphens, max 64 chars
- `description` -- required, max 1024 chars; front-load intent and trigger keywords
- `allowed-tools` -- pre-approve `Bash(dotnet:*)`, `Read`, `Write`, `Grep`, `Glob` so the workflow runs without per-tool confirmation
- `effort: high` -- these are multi-file generation tasks that benefit from deeper reasoning
- `disable-model-invocation: true` -- consider this if the skills should only run when explicitly invoked, not auto-triggered by Claude

### 2. Write descriptions for activation, not just humans

The description field is how Claude decides whether to load the skill. It should include trigger keywords and implicit contexts:

```yaml
description: >
  Create a CRUD app with list, detail, create, and edit views for Ivy.
  Use when the user mentions CRUD, master-detail, data management,
  entity management, or wants to build views for database tables.
  Handles blades, dialogs, sheets, foreign key lookups, search,
  pagination, and parent-child relationships.
```

### 3. Explicitly reference files from SKILL.md

The current skills say things like "Read the reference documents" without linking to specific files. Claude discovers files automatically, but explicit paths reduce ambiguity and ensure the right files are read:

```markdown
Read these reference files before implementing:
- [references/DashboardApp.cs](references/DashboardApp.cs) -- app structure, HeaderLayout, grid
- [references/TotalSalesMetricView.cs](references/TotalSalesMetricView.cs) -- MetricView pattern
- [references/SalesByDayLineChartView.cs](references/SalesByDayLineChartView.cs) -- chart pattern
- [references/AGENTS.md](references/AGENTS.md) -- Ivy framework API reference
```

### 4. Keep SKILL.md under 500 lines

`ivy-create-crud` is 305 lines, which is fine. But as skills grow, move detailed subsections (e.g., the "Critical Code Generation Rules" pitfalls lists) into separate reference files. The SKILL.md should contain the workflow and high-level rules; edge cases and anti-patterns belong in `references/pitfalls.md` or similar.

Target: SKILL.md body under 300 lines, reference files for depth.

### 5. Use `$ARGUMENTS` for entity/feature names

The skills could accept the user's description as an argument:

```yaml
---
name: ivy-create-crud
argument-hint: "[entity description or spec]"
---

Create CRUD views for: $ARGUMENTS
```

This lets users invoke `/ivy-create-crud Orders with OrderLines` directly instead of the skill having to ask.

### 6. Consider `context: fork` for isolation

These skills generate multiple files and have multi-step workflows. Running in a forked context (`context: fork`) would isolate the generation work from the main conversation, keeping the user's context clean:

```yaml
---
context: fork
---
```

Trade-off: forked skills return a single summary, so the user loses step-by-step visibility. This matters during the plan/review loop. For now, keep the default (inline) execution until we have a way to pause for user approval in forked context.

---

## Structural Lessons

### Workflow state machines are over-engineered for LLM tasks
The C# workflows used `AgenticTransition`, `UseExternalWidget`, and queue-based entity processing. These exist to handle the stateful, multi-turn nature of LLM conversations in a server framework. Claude Code already manages conversation state. The markdown skill just says "do step 1, then step 2" and the LLM follows.

### Prompt templates collapse into inline instructions
The original workflows had separate `.md` prompt files (`PlanDashboard.md`, `Implement.md`) that were loaded and interpolated with `{{variables}}`. In a skill, these become inline sections under each step heading. No template engine needed.

### The "entity queue" pattern from CRUD is just a list
The CRUD workflow had an explicit `EntityQueue` state variable and `NextEntityTransition` to process entities one by one. In the skill, this is just: "Implement each entity from the plan, one at a time. For each entity, create the App, ListBlade, DetailsBlade, CreateDialog, and EditSheet."

### Type alias stripping was the hardest rewrite
The reference code used `using Exa = Ivy.Internals.Workflows.References.Connections.IvyAgentExamples;` to avoid namespace collisions. The copy script had to strip these aliases and replace `Exa.` prefixes. Lesson: keep reference code clean of internal aliases; use the namespaces the skill consumers will actually see.

---

## Checklist for Future Skill Conversions

When converting another workflow to a skill:

1. **Extract the state graph** -- identify the states and transitions. Map each state to a step in the skill.
2. **Collapse prompt templates** -- inline the prompt content under each step heading. Remove template variables; use plain instructions.
3. **Copy reference files** -- add entries to `CopyReferences.ps1`. Verify namespace rewriting handles any new patterns.
4. **Add frontmatter** -- `name`, `description` (with trigger keywords), `allowed-tools`, `effort`.
5. **Link references explicitly** -- list each reference file with a markdown link and one-line description.
6. **Test the description** -- mentally run 10 queries through it: would Claude pick this skill for "build me a dashboard"? For "add a login page"? Get both true positives and true negatives right.
7. **Keep SKILL.md lean** -- workflow steps and key rules in the SKILL.md, detailed examples and edge cases in `references/`.
8. **Verify the script** -- run `CopyReferences.ps1` and confirm all files copy with 0 failures.
