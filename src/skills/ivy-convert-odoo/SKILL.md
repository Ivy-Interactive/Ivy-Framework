---
name: ivy-convert-odoo
description: >
  Convert an Odoo application or module to an Ivy project. Use when the user wants to
  migrate from Odoo, convert an Odoo module, or build an Ivy app from Odoo Python/XML
  source files. Handles Python files and folders containing Odoo modules.
allowed-tools: Bash(dotnet:*) Bash(git:*) Bash(test:*) Bash(mkdir:*) Read Write Edit Glob Grep
effort: high
argument-hint: "[path to Odoo module]"
---

# ivy-convert-odoo

Convert an Odoo application or module to an Ivy project.

## Reference Files

The [references/](references/) folder contains 60 reference files with Odoo-to-Ivy component mappings (fields, views, components). Read the relevant reference files before implementing the conversion to understand how to map Odoo features to Ivy features.

## Step 1: Locate the Odoo Module

You need a path to a Python file or a folder containing the Odoo application/module. Check if a path was provided via `$ARGUMENTS`. If not, ask the user to provide one.

Verify the path exists with `test -f "<path>"` or `test -d "<path>"`.

## Step 2: Research the Odoo Application

Read all the `.py` and `.xml` files and build a mental model of:
- Odoo models (fields.Char, fields.Many2one, fields.Selection, etc.)
- View types used (form, list/tree, kanban, calendar, graph, pivot, search, activity, etc.)
- Field widgets and OWL components
- Actions (ir.actions.act_window, server actions, automated actions)
- Business logic (compute methods, onchange, constraints, workflows)
- Security rules (ir.model.access.csv, record rules)
- Reports (QWeb templates)
- Menu structure and navigation

### 2a: Classify Each View

For each view identified, classify it:
- **CRUD**: Standard create/read/update/delete for a model (list + form)
- **Dashboard**: Reporting, charts, KPIs (pivot, graph, calendar)
- **Workflow**: Multi-step process with state transitions
- **Read-only**: Information display only, no editing

This determines the Ivy pattern to use.

### 2b: Document Security Model

Identify security and access control:
- Access rights from `ir.model.access.csv`
- Record rules (row-level security)
- `@check_access_rights` decorators
- User groups and permissions
- Document who can view/edit each model

### 2c: Map Computed Fields

For each model, identify:
- Fields with `@api.depends` decorators (computed fields)
- Fields with `@api.onchange` handlers (dynamic updates)
- `_compute_*` methods and their logic
- Dependency chains between fields
- Document the computation logic for each

### 2d: Map Menu and Navigation

Document the application structure:
- Extract menu hierarchy from `ir.ui.menu`
- Map actions from `ir.actions.act_window`
- Identify navigation flow between views
- Note sidebar, top menu, breadcrumbs

## Step 3: Detect Odoo Connection

Before converting, check if an Odoo connection is configured:

1. Look for connection files in the project's connections directory
2. Check if connection type is "odoo"
3. Verify connection details (host, database, username)
4. If missing, prompt the user to configure a connection using the appropriate connection skill (e.g., `/ivy-create-db-connection` for databases, `/ivy-create-auth-connection` for auth, `/ivy-create-any-connection` for APIs)
5. Test connection before proceeding

## Step 4: Present the Conversion Plan

Write a summarized conversion guide that maps the Odoo features used in the application to Ivy features. The conversion guide should be structured in a way that makes it easy to follow when implementing the conversion. Use markdown formatting to make it clear and organized -- but be concise and token efficient.

Present the plan to the user for approval before proceeding.

## Step 5: Implementation

Identify if there are any additional connections (db, auth, api) that should be set up using the appropriate connection skill (e.g., `/ivy-create-db-connection` for databases, `/ivy-create-auth-connection` for auth, `/ivy-create-any-connection` for APIs).

Given the conversion guide from the previous step, implement the conversion of the Odoo application to an Ivy application. Use the conversion guide and the reference files to map Odoo features to Ivy features.

## Step 6: Validation

After generating Ivy code, validate the conversion:

1. All Odoo views have corresponding Ivy pages
2. All models have Ivy data classes
3. All form fields have Ivy input widgets
4. Security rules are documented (even if not implemented)
5. Computed fields have UseQuery or UseEffect equivalents
6. Menu structure maps to Ivy routing

Generate a validation report listing:
- Successfully converted views
- Views with workarounds/limitations
- Missing or incomplete conversions
