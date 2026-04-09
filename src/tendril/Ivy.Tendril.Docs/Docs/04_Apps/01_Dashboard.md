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
The Dashboard provides a panoramic, high-level overview of an entire Tendril ecosystem. It tracks cumulative plan states, live cost estimations, token charts, and immediate activity across all integrated projects.
</Ingress>

## Executive Overview

The Dashboard is the default landing app when you boot Tendril. It gives you instant situational awareness using the following visual layouts:

- **Plan Count Statistics** — A stacked progress bar visually demonstrating the health pipeline of your repository (e.g. how many plans are in Draft, Executing, Review, or Completed).
- **Cost and Token Analytics** — Rendered, real-time Bar charts evaluating system spend rates. Identifies potential AI-budget drain dynamically.
- **Activity Feed** — A detailed diagnostic table of all recently mutated plans detailing Status, target Project, exact Job Cost, and creation Timestamps.

## Deep-Filtering Navigation

The Dashboard isn't static—it's fully interactable:

- **By Project** — Select a designated project segment from the Stacked Progress visualizer, instantly constraining the entire view to metrics associated only with that codebase.
- **By Time Period** — Use the bounding constraints to specify "Last 24 Hours", "This Week", or discrete data focuses.

## Cost Architecture

The dashboard compiles financial metrics directly from the root filesystem. Every executed job continuously aggregates and dumps a standardized row to the `costs.csv` file within a Plan's secure folder. 

When the Dashboard initializes, it recursively sweeps these files within `TENDRIL_HOME` to generate granular burn-down charts sliced by:
- Targeted Project (automatically color-coded by the Setting App preferences)
- Promptware Runtime (Evaluating whether `ExecutePlan` or `MakePlan` consumed the most volume)
- Raw input versus Raw output tokens.
