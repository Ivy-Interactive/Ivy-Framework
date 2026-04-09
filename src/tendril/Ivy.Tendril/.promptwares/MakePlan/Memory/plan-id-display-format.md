# Plan ID Display Format

## User Preference

**CRITICAL:** When displaying plan IDs in Tendril UI list items, the word "plan" MUST be visible.

**Correct format:** `"plan #1992 Title"`  
**Incorrect format:** `"#1992 Title"` ❌

## Rationale

The user has requested this fix 5+ times (as of 2026-04-06). Without the word "plan", the number lacks context and is harder to parse visually.

## Implementation Pattern

In all sidebar ListItem titles:

```csharp
// ✅ CORRECT
return new ListItem($"plan #{plan.Id} {plan.Title}")

// ❌ WRONG - missing "plan" prefix
return new ListItem($"#{plan.Id} {plan.Title}")
```

## Affected Files

As of 2026-04-06, these files display plan IDs in sidebar lists:
- `Ivy.Tendril/Apps/Plans/SidebarView.cs`
- `Ivy.Tendril/Apps/Review/SidebarView.cs`
- `Ivy.Tendril/Apps/Icebox/SidebarView.cs`
- `Ivy.Tendril/Apps/Recommendations/SidebarView.cs`

When creating new apps or views that display plan IDs, follow the "plan #XXXX" format.

## Historical Context

- Plan 01810 (2026-04-05) moved plan IDs between badge and title but didn't address the missing "plan" prefix
- This issue was reported 5 times before plan 02009 was created to fix it comprehensively

## Verification

After making changes to plan ID display code, visually verify in the running app that:
1. Drafts page shows "plan #XXXX Title"
2. Review page shows "plan #XXXX Title"
3. Icebox page shows "plan #XXXX Title"
4. Recommendations page shows "plan #XXXX Title"
