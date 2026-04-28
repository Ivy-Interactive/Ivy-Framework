---
name: ivy-convert-retool
description: >
  Convert a Retool application to an Ivy project. Use when the user wants to migrate
  from Retool, convert a Retool app, or build an Ivy app from an exported Retool
  project (.zip file). Uses the ivy-inspector-retool CLI tool to inspect exported
  Retool projects.
allowed-tools: Bash(ivy-inspector-retool:*) Bash(dotnet:*) Bash(git:*) Bash(test:*) Bash(mkdir:*) Read Write Edit Glob Grep
effort: high
argument-hint: "[path to .zip file]"
---

# ivy-convert-retool

Convert an exported Retool project to an Ivy project.

## Reference Files

The [references/](references/) folder contains 134 reference files with Retool-to-Ivy component mappings (one `.md` per Retool widget). Read the relevant reference files before implementing the conversion to understand how to map Retool features to Ivy features.

## Step 1: Locate the Retool Export

You need a path to a `.zip` file containing an exported Retool project. Check if a path was provided via `$ARGUMENTS`. If not, ask the user to provide one.

Verify the file exists using `test -f "<path>"`.

## Step 2: Research the Retool Application

Use the `ivy-inspector-retool` CLI tool to inspect the content of the zip file. This tool is already installed.

1. Run `ivy-inspector-retool docs` to see all available commands and options.
2. Use ivy-inspector-retool to inspect the zip file. Issue multiple Bash commands in parallel to gather information as fast as possible.

Your goal is to analyze the application built in Retool and compile a detailed report so that it can be converted to an Ivy application. Answer these questions:

- What are the data sources used in the Retool application? For each data source, what type is it (e.g. REST API, SQL database, GraphQL, etc), what are the endpoints or tables used, and what queries are made?
- What are the pages in the Retool application? What are their names and purposes? A page in Retool will be converted to an App in Ivy.
- What are the modals (dialogs) and drawers (sheets)?

## Step 3: Present the Conversion Plan

Given the research output, present the user with a plan using the following structure:

For each app, assign a **Type:**
- `CRUD` -- When the app is backed by a database table containing entity records that users would create, edit, delete
- `Dashboard` -- For summary, analytics, or reporting views
- `Ad-hoc` -- For utility or custom apps that don't fit the above categories

```markdown
# Retool to Ivy Conversion Plan

**File:** [path to zip file]

## Data Sources

### DataSource:MyRestApi

**Type:** REST API
**Base URL:** https://api.example.com/v1
**Authentication:** Bearer token
**Endpoints:**
- GET /customers -- list all customers
- GET /customers/:id -- get customer by ID
- POST /customers -- create customer
- ...

### DataSource:MyDatabase

**Type:** SQL (PostgreSQL)
**Tables:** customers, orders, products
**Key Queries:**
- SELECT * FROM customers WHERE active = true
- ...

## Pages

### Page:Customers -> App:Customers

**File:** /Apps/CustomersApp.cs
**Type:** CRUD
**Icon:** User
**Data Source:** MyRestApi (GET /customers, POST /customers)
**Description:**
Lists all customers with search and filtering. Allows creating and editing customer records.

### Page:Dashboard -> App:Dashboard

**File:** /Apps/DashboardApp.cs
**Type:** Dashboard
**Icon:** Dashboard
**Data Source:** MyDatabase (aggregation queries)
**Description:**
Summary view showing key metrics and charts.

### Page:ImportTool -> App:ImportTool

**File:** /Apps/ImportToolApp.cs
**Type:** Ad-hoc
**Icon:** Upload
**Description:**
Utility page for importing CSV data into the system.

## Modals and Drawers

### Modal:EditCustomer

**Triggered from:** Customers page
**Purpose:** Edit form for a single customer record

### Drawer:CustomerDetails

**Triggered from:** Customers page
**Purpose:** Side panel showing full customer details

## Connection Configuration

**Connection Name:** MyCompany
**Namespace:** MyCompany.Connections.MyCompany
**Secrets:**
- MyCompany:ApiKey
- MyCompany:BaseUrl

## Other Notes

...

```

## Step 4: Implementation

Implement the plan approved by the user in the previous step.
