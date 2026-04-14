---
icon: Github
searchHints:
  - github
  - issues
  - pull requests
  - prs
  - import
---

# GitHub

<Ingress>
Tendril integrates with GitHub for issue importing, automatic PR creation, and PR status tracking.
</Ingress>

## Authentication

Tendril uses the GitHub CLI (`gh`) for authentication. Run `gh auth login` to authenticate before using GitHub features.

<Callout type="info">
Ensure `gh` is installed and available on your PATH. Tendril will prompt you during onboarding if it is missing.
</Callout>

## Importing Issues

Use the **Import Issues** dialog to fetch GitHub issues and convert them into Tendril plans:

1. Open the **Plans** app
2. Select **Import Issues** from the menu
3. Choose the target repository and filter by labels or milestones
4. Selected issues are converted into plans via the MakePlan promptware

Each imported issue retains a link back to the original GitHub issue for traceability.

## Automatic PR Creation

When a plan reaches the **Completed** state, the **MakePr** promptware automatically:

1. Pushes the worktree branch to the remote
2. Creates a pull request with a summary of changes
3. Links the PR back to the plan

PR settings like merge strategy are controlled by the `prRule` field in your project configuration.

## PR Status Sync

The **PrStatusSyncService** monitors open PRs and updates plan state when PRs are merged or closed. This keeps your plan board in sync with your repository without manual intervention.
