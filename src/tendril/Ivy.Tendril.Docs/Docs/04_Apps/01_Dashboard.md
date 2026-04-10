---
searchHints:
  - dashboard
  - statistics
  - overview
  - charts
  - cost
icon: ChartBar
---

# Dashboard

<Ingress>
Landing view: plan counts, spend and tokens, and recent activity across projects.
</Ingress>

## What you see

- **Plan counts** — Stacked bar by state (Draft, in flight, Review, Completed, …).
- **Cost / tokens** — Bars for burn rate and where spend goes.
- **Activity** — Recent plan changes with status, project, cost, timestamps.

## Filters

- **Project** — Click a segment in the stacked bar to scope metrics to one repo.
- **Time** — e.g. last 24 hours, this week.

## Where costs come from

Each job appends rows to that plan’s `costs.csv`. The Dashboard reads these under `TENDRIL_HOME` and charts by project (colors from Settings), promptware type, and input vs. output tokens.
