---
searchHints:
  - overview
  - what-is
  - tendril
  - agent
  - orchestration
  - architecture
---

# Welcome to Tendril

<Ingress>
Tendril is an advanced, multi-host AI orchestration platform designed to automate and manage complex software development lifecycles. Built on the Ivy Framework, it blends an intuitive cross-platform desktop interface with autonomous agent capabilities.
</Ingress>

Tendril gives you full visibility and control over your AI-assisted development workflow. By breaking down engineering tasks into discrete, transparent stages, it orchestrates Claude-based agents to perform high-fidelity code generation, verification, and pull request management without hiding the details from you.

<Embed Url="https://www.youtube.com/watch?v=PLACEHOLDER"/>

## The Concept

At its core, Tendril is built around **Plans**. A Plan is a structured representation of a desired change—whether it's fixing a bug, refactoring a module, or building a new feature. Tendril manages these Plans through a rigorous lifecycle, passing them to highly specialized, isolated AI agents called **Promptwares**.

## Key Features

- **Plan Lifecycle Management** — Robust tracking of work across stages: Draft, Executing, Review, and PR generation.
- **Multi-Project Orchestration** — Configure and run agents across multiple repositories simultaneously, with per-project verification rules.
- **High-Fidelity Agent Jobs** — Live monitoring of agent status, execution time, token usage, and cost tracking.
- **Specialized Promptwares** — Purpose-built routines that govern specific tasks like `MakePlan`, `ExecutePlan`, `ExpandPlan`, and `MakePr`.
- **Integrated Git Worktrees** — Safely isolates AI execution within git worktrees, ensuring your primary development branch remains untouched.
- **Built-In Terminal & Tools** — A fully-featured embedded terminal (running Claude Code underneath) and a fast local File Viewer.
- **Advanced Verification** — Native integration with your build, test, and formatting pipelines to automatically verify AI-generated code.

## The Tendril Loop

Tendril is designed to augment, not replace, the developer. The core loop looks like this:

1. **Ideation (`MakePlan`)**: You provide a prompt or an issue URL. The agent drafts a detailed, multi-step implementation plan.
2. **Expansion (`ExpandPlan`)**: For large features, Tendril can automatically split and expand the plan into smaller, executable chunks.
3. **Execution (`ExecutePlan`)**: The agent creates an isolated git worktree, implements the solution, builds it, runs your tests, and iterates until the verifications pass.
4. **Human Review (`Review`)**: You inspect the final diff and run the application. You can approve the work, or reject it with feedback for the agent to try again.
5. **Integration (`MakePr`)**: Once approved, Tendril automatically creates a GitHub Pull Request with the completed work.

By structuring the workflow this way, Tendril transforms AI from a basic code autocomplete tool into an autonomous engineering partner.

## Quick Install 

The easiest way to install Tendril and all of its required components on macOS or Linux is via our streamlined install script:

```bash
curl -sSf https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/src/tendril/install.sh | sh
```
