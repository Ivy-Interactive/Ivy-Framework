---
searchHints:
  - review
  - diff
  - approve
  - reject
  - verification
icon: ThumbsUp
---

# Review

<Ingress>
The Review app is where you inspect completed plans, review diffs, check verification results, and approve or send plans back for revision.
</Ingress>

## Review Workflow

When a plan completes execution, it moves to the **Review** state. The Review app shows:

1. **Summary** — The plan's description, problem statement, and proposed solution
2. **Commits** — List of commits made during execution with diff views
3. **Verifications** — Results of build, format, and test checks
4. **Artifacts** — Screenshots and other outputs from the execution
5. **Recommendations** — AI-generated suggestions for improvements

## Actions

From the Review app, you can:

| Action | Description |
|--------|-------------|
| **Approve** | Mark the plan as ready for PR creation |
| **Suggest Changes** | Send the plan back to Draft with feedback |
| **Discard** | Move the plan to Trash |
| **Make PR** | Create a GitHub pull request directly |
| **Open in VS Code** | Open the worktree in your editor |
| **Open in Terminal** | Open a terminal at the worktree path |

## Verification Results

Each verification shows a pass/fail/skip status. Click on a verification to see detailed output. Failed verifications are highlighted and should be addressed before creating a PR.
