---
searchHints:
  - pull requests
  - pr
  - merge
  - github
icon: GitPullRequest
---

# Pull Requests

<Ingress>
Track and open GitHub PRs from Tendril after Review approves **MakePr**.
</Ingress>

## Flow

Work stays on **branches / worktrees**, not your random dirty tree. After approval, **MakePr** builds the PR via `gh` from the diff.

## In the app

- **Open** — AI-opened PR awaiting review or CI.
- **Merged** — Landed on the default branch.

You can **merge** from here when checks pass. If CI fails, fix in GitHub or locally and use the usual PR workflow.
