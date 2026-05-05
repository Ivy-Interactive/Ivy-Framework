---
name: ivy
description: >
  Master skill for building Ivy Framework applications. Use when the user asks to
  build something with Ivy, create an app, add a feature, fix a bug, or work on
  their Ivy project. Routes to specialized skills for CRUD, dashboards, connections,
  conversions, themes, widgets, and deployment. Also provides direct access to the
  Ivy CLI for documentation lookups and framework questions.
allowed-tools: Bash(dotnet:*) Bash(ivy:*) Read Write Edit Glob Grep
effort: medium
---

# Ivy

Master skill for building applications with the Ivy Framework.

## Pre-flight: Read Learnings

If the file `.ivy/learnings/ivy.md` exists in the project directory, read it first and apply any lessons learned from previous runs of this skill.

## Reference Files

- [references/AGENTS.md](references/AGENTS.md) -- Ivy framework API reference (widgets, hooks, layouts, inputs, colors)

Read the AGENTS.md reference for any direct implementation work. For specialized tasks, delegate to the appropriate skill below.

## Ivy CLI

Use the `ivy` CLI for documentation lookups and framework questions:

| Scenario | Command |
|---|---|
| You know the topic and want the full reference page | `ivy docs <path>` |
| You need to browse what documentation exists | `ivy docs list` |
| You have a "how do I..." question and need a synthesized answer | `ivy ask "your question"` |
| You need to understand an existing connection's schema | `ivy cli explain connections/<ConnectionName>` |

## Skill Router

When the user's request matches a specialized task, delegate to the appropriate skill instead of implementing from scratch.

### Build Apps

| User wants... | Skill |
|---|---|
| CRUD views, master-detail, data management for database tables | `/ivy-create-crud` |
| Dashboard with metrics, charts, KPIs, analytics | `/ivy-create-dashboard` |
| Custom app (chat, wizard, tool, streaming UI, bespoke layout) | `/ivy-create-app` |

### Add Connections

| User wants... | Skill |
|---|---|
| Connect to an existing database (SQL Server, Postgres, MySQL, SQLite, etc.) | `/ivy-create-db-connection` |
| Generate a new database with AI-designed schema and seeding | `/ivy-generate-db-connection` |
| Add authentication (Auth0, Supabase, Clerk, Basic, GitHub, etc.) | `/ivy-create-auth-connection` |
| Connect to a REST/OpenAPI endpoint | `/ivy-create-openapi-connection` |
| Connect to a GraphQL API | `/ivy-create-graphql-connection` |
| Connect to a SOAP/WSDL service | `/ivy-create-soap-connection` |
| Use a pre-built connection from the Ivy catalog | `/ivy-create-using-reference-connection` |
| Connect to any other API or service | `/ivy-create-any-connection` |

### Convert from Other Platforms

| User wants... | Skill |
|---|---|
| Convert from Airtable | `/ivy-convert-airtable` |
| Convert from Excel (.xlsx) | `/ivy-convert-excel` |
| Convert from Lovable (React/Supabase) | `/ivy-convert-lovable` |
| Convert from Odoo (Python/XML) | `/ivy-convert-odoo` |
| Convert from Reflex (Python) | `/ivy-convert-reflex` |
| Convert from Retool (.zip export) | `/ivy-convert-retool` |
| Convert from Streamlit (Python) | `/ivy-convert-streamlit` |

### Customize and Deploy

| User wants... | Skill |
|---|---|
| Create or modify a theme (colors, typography, styling) | `/ivy-create-theme` |
| Create a custom React-backed widget (wrap npm package) | `/ivy-create-external-widget` |
| Deploy as a desktop application (.exe) | `/ivy-deploy-to-desktop` |

## Direct Implementation

If the user's request does not match any specialized skill above -- for example, modifying existing views, fixing bugs, adding features to existing apps, or general Ivy development -- implement it directly using the AGENTS.md reference and the `ivy` CLI for documentation.

## Post-run: Evaluate and Improve

After completing the task:

1. **Evaluate**: Did the build succeed? Were there compilation errors, unexpected behavior, or manual corrections needed during this run?
2. **Update learnings**: If anything required correction or was surprising, append a concise entry to `.ivy/learnings/ivy.md` (create the file and `.ivy/learnings/` directory if they don't exist). Each entry should note: the date, what went wrong, why, and what to do differently next time.
3. **Skip if clean**: If everything succeeded without issues, do not update the learnings file.
