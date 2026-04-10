# Plan Expansion Patterns

## What Makes a Plan "Concrete"

A concrete plan has:
- ✅ Exact file paths with line numbers
- ✅ Specific code changes (not "fix the issue")
- ✅ Referenced patterns from existing codebase
- ✅ Root cause analysis for bugs
- ✅ Clear decision points with options when multiple approaches exist
- ✅ Impact analysis on other components
- ✅ Specific verification steps

A plan is NOT concrete if it has:
- ❌ "Investigate...", "Check if...", "Verify whether..."
- ❌ "TODO: decide", "Need to determine", "Figure out"
- ❌ Vague instructions like "update as needed", "make it work"
- ❌ Missing file paths or "somewhere in the codebase"
- ❌ Assumptions that haven't been verified

## Research Depth Requirements

### Minimum Research for Any Plan:
1. Read all directly mentioned files
2. **Check for plural nouns in title** ("DataTables", "Components", "Forms") — signals multiple instances to find
3. Search for similar patterns in codebase (use `Find-SimilarPatterns.ps1 -FileTypes cs,tsx,ts` or Grep)
4. Verify all file paths and line numbers exist
5. Check if related tests exist

### Additional Research for Bug Fixes:
1. Reproduce the issue mentally by tracing code flow
2. Identify root cause (not just symptoms)
3. Find how similar bugs were fixed previously
4. Check if issue already fixed in recent commits

### Additional Research for New Features:
1. Find similar features in codebase
2. Identify architectural patterns to follow
3. Check documentation for conventions
4. Determine what tests are needed

### Additional Research for Refactoring:
1. Identify all usages of code being changed
2. Determine breaking changes
3. Find all affected tests
4. Check if similar refactors happened before

## Decision Point Template

When multiple approaches are valid:

```markdown
## DECISION REQUIRED: [Short Title]

### Option A: [Name] (RECOMMENDED if one is clearly better)
- **Approach:** [Specific implementation steps]
- **Pros:**
  - [Based on research: aligns with pattern X in FileY.cs]
  - [Performance/maintainability/etc.]
- **Cons:**
  - [Honest tradeoffs]
- **Effort:** X files, ~Y lines of code
- **Precedent:** Used in [ComponentA, ComponentB]

### Option B: [Name]
- **Approach:** [Specific implementation steps]
- **Pros:**
  - [Based on research]
- **Cons:**
  - [Honest tradeoffs]
  - [Why it's not recommended if applicable]
- **Effort:** X files, ~Y lines of code
- **Precedent:** [None | Used in ComponentC]

**Recommendation:** Option A because [research-backed reason]
```

## Common Pitfalls

### Pitfall: "Check if X exists"
❌ Bad: "Check if StateManager class exists"
✅ Good: "StateManager class exists at `src/Ivy/State/StateManager.cs:45`. It provides..."

### Pitfall: Vague locations
❌ Bad: "Update the dialog component"
✅ Good: "In `src/Ivy/Components/Dialog.razor.cs:147`, modify the StateHasChanged() call"

### Pitfall: Missing patterns
❌ Bad: "Add error handling"
✅ Good: "Add try-catch following pattern in `TableWidget.tsx:89`, log errors using Logger.Error()"

### Pitfall: Unverified assumptions
❌ Bad: "The API should return JSON"
✅ Good: "Verified: API returns JSON (see `ApiClient.cs:234` deserialization logic)"

### Pitfall: No impact analysis
❌ Bad: "Change the Button component"
✅ Good: "Change Button component. Used by 47 components. Breaking change: add migration guide."

## Edge Case: Plans That Appear Concrete But Aren't

Sometimes a plan has no investigative language but still needs expansion because:

1. **Technical inaccuracies**: Solution describes changes incorrectly
   - Example: "Field will auto-update from constructor" (wrong: C# requires explicit type)
   - Action: Read the actual file, verify all statements, correct inaccuracies

2. **Incomplete solutions**: Solution misses necessary changes
   - Example: Changes constructor but forgets field declarations
   - Action: Trace through all affected code, list ALL changes needed

3. **Unverified audit claims**: Says "checked all X" but details are missing
   - Example: "All dialogs audited" but no evidence shown
   - Action: Spot-check claims by reading sample files, verify accuracy

**When to expand these:**
Even if no "Investigate..." language exists, create an expanded revision if the solution would fail or confuse the implementer.

## Quality Checklist

Before saving expanded plan, verify:
- [ ] No investigative language (investigate, check, verify, research, explore, TODO, decide)
- [ ] No vague terms (update as needed, fix the issue, make it work)
- [ ] All file paths absolute with line numbers
- [ ] All assumptions verified or corrected
- [ ] Design decisions resolved or presented with concrete options
- [ ] Developer can implement without opening files for research
- [ ] Impact analysis included (what else affected)
- [ ] Verification steps specific and actionable
- [ ] **All technical claims verified by reading actual source files**
- [ ] **Solution includes ALL necessary changes (not just the obvious ones)**
