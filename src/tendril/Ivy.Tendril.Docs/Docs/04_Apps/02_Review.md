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
Queue of finished work: **ReadyForReview** or **Failed** plans. Nothing merges without you.
</Ingress>

## What shows up

`ExecutePlan` (and similar) finishes in a frozen worktree. Review lists those plans and shows the result here.

## In the panel

- **Diff** — Changes vs. your tracked branch.
- **Verification output** — Logs from hooks (`DotnetBuild`, `NpmTest`, …).
- **Plan text** — Latest `revisions/*.md` (what the agent was implementing).

## Actions

| Action | Effect |
|--------|--------|
| **Approve (Make PR)** | Marks **Completed**, starts **MakePr** on GitHub. |
| **Needs work (Revise)** | **UpdatePlan** on the same worktree with your feedback. |
| **Decline (Discard)** | **Skipped**, worktree removed. |
| **Manually resolve** | Edit files in the workspace for small fixes without another full agent loop. |
