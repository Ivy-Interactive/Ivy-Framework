---
name: GraphQL Merge Fallback
description: When gh pr merge fails with 502, "already in progress", worktree conflicts, or "Base branch was modified", use REST API as fallback
type: feedback
---

When `gh pr merge` (GraphQL) returns a 502, gets stuck in "Merge already in progress" state, fails with worktree conflicts ("'main' is already used by worktree"), or fails with "Base branch was modified", fall back to the REST API immediately:

```bash
gh api repos/<owner>/<repo>/pulls/<number>/merge -X PUT -f merge_method=merge
```

Then delete the branch separately:
```bash
gh api repos/<owner>/<repo>/git/refs/heads/<branch> -X DELETE
```

**Why:** GitHub's GraphQL merge endpoint occasionally times out (502) but queues the merge internally. Retrying GraphQL then returns "already in progress" indefinitely. The REST API bypasses this stuck state.

**How to apply:** After 1 failed `gh pr merge` attempt, switch to REST API rather than retrying GraphQL with sleeps.
