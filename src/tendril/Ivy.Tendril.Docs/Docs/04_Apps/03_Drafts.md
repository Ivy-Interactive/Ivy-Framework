---
searchHints:
  - draft
  - plan
  - ideation
  - blocked
  - makeplan
icon: Feather
---

# Drafts

<Ingress>
The Drafts application (Internally tracked as `PlansApp`) serves as the ideation funnel for Tendril. It collects raw user requirements and translates them into actionable architectural guidelines.
</Ingress>

## Purpose

Any new Plan born from the Tendril environment initializes in the `Draft` state (or `Blocked` if awaiting clarification). This view ensures that humans and promptwares collaborate actively to refine requirements before writing a single line of application source code.

## The Draft Interface

The view contains a real-time reactive Sidebar tracking filtered Draft states against the main reading pane.

When viewing an active draft, the tool presents:

- **The Objective File**: Sourced directly from `revisions/revision-{idx}.md` outlining the structured Problem, Architectural Solution, and target Tests.
- **Project Overrides**: Read-outs of designated `config.yaml` target parameters governing execution boundaries.

## Plan Action Triggers

The Draft view governs the invocation triggers that physically turn text into running agents:

1. **Launch ExecutePlan** — Commits the current drafted revision format, establishes the headless git worktree isolation sandbox, and dispatches the main execution Agent.
2. **Launch ExpandPlan** — Interrogates a thin architectural concept through a heavier Promptware specifically designed to inject rigid implementation specifics if the user hasn't provided enough guidance.
3. **Shelve to Icebox** — Sends an active Draft directly to cold storage (`Icebox`) reducing UI clutter.

## Synchronization

Because Tendril is tightly integrated with the Host OS infrastructure, you can manually compose an `.md` document inside the `TENDRIL_HOME/Inbox` folder, or modify an existing Draft markdown with your favorite IDE (like VSCode). The Drafts App utilizes active filesystem watchers to propagate external text modifications onto your graphical interface recursively without needing manual refreshes.
