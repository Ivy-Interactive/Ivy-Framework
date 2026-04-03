# CreateIssue

Create a GitHub issue from a plan.

## Context

The firmware header contains:
- **PlanFolder** — path to the plan folder
- **ConfigPath** — absolute path to config.yaml
- **CurrentTime** — current UTC timestamp
- **Repo** — target repository path (local path)
- **Assignee** — GitHub username to assign (optional, may be empty)
- **Comment** — optional comment to include in the issue body (may be empty)

## Execution Steps

### 1. Read Plan

- Read `plan.yaml` from the plan folder
- Read the latest revision for the plan title and Problem section

### 2. Identify GitHub Repository

From the `Repo` path:
```bash
cd <Repo>
gh repo view --json nameWithOwner --jq ".nameWithOwner"
```

If this fails, report that the repo is not a GitHub repository and stop.

### 3. Create Issue

Use the plan's title and Problem section to create a well-formatted issue:

```bash
gh issue create --repo <owner/repo> --title "<title>" --body "<body>"
```

- **Title:** Plan title
- **Body:** Markdown-formatted from the plan's Problem section, with a link back to the plan ID. If `Comment` is non-empty, append it to the body under an "**Additional context:**" heading separated by a horizontal rule (`---`).
- **Assignee:** If provided, add `--assignee <Assignee>`

### 4. Update plan.yaml

The issue URL should be noted in the output for the user.

### Rules

- Do NOT modify any source code
- Use `gh` CLI for all GitHub operations
- If the repo has no GitHub remote, fail with a clear message
