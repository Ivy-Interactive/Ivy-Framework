# Frontend vs Backend Terminology

**Created**: 2026-04-06 (Session 00820)

## The Context

When creating plan #1986 to fix bar chart data gaps, revision 002 moved the solution from Tendril backend services to Ivy Framework's `PivotTable.cs` - but this was still BACKEND C# code, not the actual frontend.

User feedback: "This not not a Tendril issue the bugs are in Ivy FE" - but UpdatePlan still targeted backend code.

## The Clarification

In this codebase:

### Frontend (FE)
- **Location**: `src/frontend/` directory
- **Languages**: TypeScript, React, TSX
- **Examples**: 
  - `BarChartWidget.tsx`
  - `sharedUtils.ts`
  - `LineChartWidget.tsx`
- **Purpose**: Client-side UI rendering, chart rendering, user interactions

### Backend (BE)
- **Location**: `src/Ivy/`, `src/tendril/Ivy.Tendril/`
- **Languages**: C#
- **Examples**:
  - `PivotTable.cs` (Ivy Framework backend)
  - `PlanReaderService.cs` (Tendril backend)
  - `BarChartView.cs` (Ivy Framework backend)
- **Purpose**: Server-side logic, data processing, API endpoints

### "Framework" Ambiguity

The term "Ivy Framework" includes BOTH:
- Backend: `src/Ivy/` (C# classes like `BarChartView`, `PivotTable`)
- Frontend: `src/frontend/` (React components like `BarChartWidget`)

When user says "the bug is in FE", they specifically mean the **TypeScript/React code** in `src/frontend/`, NOT the C# framework code.

## How to Apply

When creating plans:

1. **"Fix in frontend" / "FE issue"** → Target TypeScript/React files in `src/frontend/`
2. **"Fix in backend" / "BE issue"** → Target C# files in `src/Ivy/` or service layers
3. **"Fix in Tendril"** → Target C# files in `src/tendril/Ivy.Tendril/`

If uncertain which layer has the bug:
- Ask in `## Questions` section
- Trace data flow from backend → frontend to identify the actual source

## Example from Session 00820

**Problem**: Bar chart showing data gaps

**Data flow**:
1. Backend sends: `[{ Hour: "03/31 07", Cost: 10 }, { Hour: "03/31 09", Cost: 15 }]`
2. Frontend `sharedUtils.ts:131` extracts categories: `["03/31 07", "03/31 09"]`
3. Frontend `BarChartWidget.tsx:180` plots only those categories
4. ECharts renders gaps

**Root cause**: Frontend data preparation doesn't fill time-series gaps

**Correct solution**: Fix `sharedUtils.ts` and `BarChartWidget.tsx` (FRONTEND)

**Incorrect solution**: Fix `PivotTable.cs` (BACKEND - backend can't fix frontend rendering)

## Related Memory

- `Memory/worktree-frontend-build-pattern.md` — Frontend build patterns for isolated testing
