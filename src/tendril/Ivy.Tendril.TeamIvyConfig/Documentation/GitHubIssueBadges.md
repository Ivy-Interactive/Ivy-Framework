# GitHub Issue Badges Investigation

**Date:** 2026-04-13  
**Plan:** 03212-What-are-these-GitHub-issue-badges

## Summary

The badges observed on GitHub issues/PRs #3205, #3208, and #3211 in the Ivy-Framework repository are **GitHub Projects custom field values**, not standard GitHub labels.

## Findings

### Issue Metadata (via GitHub API)

| # | Title | Type | Labels | Comments | State |
|---|-------|------|--------|----------|-------|
| 3205 | [01943] Fix remaining service interface usage in Tendril apps | PR | None | 1 | Merged |
| 3208 | [01945] Extract IModelPricingService interface... | PR | None | 1 | Merged |
| 3211 | [01951] Refactor PlanReaderService caches into generic TimeCache\<T\> | PR | None | 1 | Merged |

All three items are **merged pull requests**, not issues. None have GitHub labels assigned.

### Badge Analysis

| Badge | Likely Source | Explanation |
|-------|-------------|-------------|
| **"2"** on #3205 | GitHub Projects custom field | Sprint/iteration number, story points, or another numeric project field |
| **"P0"** on #3208 | GitHub Projects custom field | Priority field value (Priority 0 = highest) |
| **"S"** on #3211 | GitHub Projects custom field | Size estimation field (S = Small) |

### What Are GitHub Projects Custom Fields?

GitHub Projects (v2) allows defining custom fields on project boards:

- **Priority** - Single select (P0, P1, P2, P3)
- **Size** - Single select (XS, S, M, L, XL)  
- **Sprint/Iteration** - Iteration field (numeric)
- **Status** - Single select (Todo, In Progress, Done)

When viewing the issue/PR list on GitHub, these custom field values appear as small colored badges next to the title — in addition to standard labels.

### What They Are NOT

- **Not GitHub labels** - API confirms all three items have zero labels
- **Not comment counts** - All three have exactly 1 comment each (doesn't match the "2" badge)
- **Not review counts** - #3208 has zero reviews but shows "P0"
- **Not Tendril labels** - "Tendril" and "NiceToHave" referenced in the plan description are Tendril's own `project` and `level` metadata (from `plan.yaml`), not GitHub labels. Tendril's `MakePr` promptware does not add labels when creating PRs via `gh pr create`.

### Token Scope Limitation

The current `gh` token lacks the `read:project` scope required to query GitHub Projects fields via the GraphQL API. This prevented direct API confirmation. Adding this scope would allow:

```graphql
{
  repository(owner: "Ivy-Interactive", name: "Ivy-Framework") {
    projectsV2(first: 10) {
      nodes { id title number }
    }
  }
}
```

### Repo Labels (for reference)

The repository has these labels defined: `API review required`, `api-review-required`, `bug`, `claude`, `dependencies`, `enhancement`, `good first issue`, `help wanted`, `javascript`, `on HOLD`, `prioritized`, `tendril`.

Note: There is no `NiceToHave` or `P0` label on the repository.

## How Tendril Displays GitHub Issues

The `ImportIssuesDialog.cs` fetches and displays GitHub issues with minimal metadata:
- Shows issue number and title: `#{i.Number} — {i.Title}`
- Allows filtering by repo, search query, assignee, and labels
- Does **not** display comment counts, project field values, or other metadata

## Recommendations

1. **Add `read:project` scope** to the GitHub token to confirm the Projects field hypothesis and potentially expose project metadata in Tendril's UI
2. **Enhance ImportIssuesDialog** to display additional metadata (comment count, labels) when listing issues — currently only shows number and title
3. **Consider showing Tendril metadata** (level, project) as badges when displaying plans that originated from GitHub issues
