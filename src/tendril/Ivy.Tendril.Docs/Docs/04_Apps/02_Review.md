---
searchHints:
  - review
  - approve
  - reject
  - diff
  - verify
icon: ThumbsUp
---

# Review

<Ingress>
The Review app guarantees that no AI-generated code is ever integrated blindly. It presents you with an actionable queue of finished executions ready for human evaluation.
</Ingress>

## Objective

The Review App continuously aggregates all Plans that have successfully entered the `ReadyForReview` or `Failed` status. Once an agent job like `ExecutePlan` finishes its operation, Tendril freezes the background git worktree and serves the generated payload up within this panel.

## The Review Interface

Selecting an item from the Review Sidebar exposes a comprehensive breakdown pane containing:

1. **The Core Diff**: A syntax-highlighted snapshot of the exact code delta proposed by the Agent against your repository's tracked branch.
2. **Terminal Verifications**: Direct outputs of executed validation hooks (`DotnetBuild`, `NpmTest`).
3. **Task Instructions**: The AI’s stated intent derived from the `revisions/*.md` trace.

## Human Interventions

From the Action Bar, you can dictate the outcome of the proposed work:

| Action | Result |
|--------|--------|
| **Approved (Make PR)** | Greenlights the plan content, shifting state to `Completed` and launching the asynchronous `MakePr` job via GitHub. |
| **Needs Work (Revise)** | Rejects the implementation. Bootstraps the `UpdatePlan` job recursively to command the AI to iterate over the identical worktree based on human guidance. |
| **Decline (Discard)** | Shelves the execution definitively, transitioning the Plan state quietly to `Skipped` and deleting the associated worktree to free resources. |
| **Manually Resolve** | Transitions into a File Edit state allowing direct human patching of minor logical faults inside the workspace without requiring AI cycle looping. |
