---
name: ivy-convert-airtable
description: >
  Convert an Airtable base to an Ivy project. Use when the user wants to migrate
  from Airtable, convert an Airtable base, import Airtable data, or build an Ivy
  app from an Airtable base. Requires Base ID and Personal Access Token (PAT).
  Uses the ivy-inspector-airtable CLI tool to inspect and extract base metadata.
allowed-tools: Bash(ivy-inspector-airtable:*) Bash(dotnet:*) Bash(git:*) Bash(test:*) Bash(mkdir:*) Read Write Edit Glob Grep
effort: high
argument-hint: "[Airtable Base ID]"
---

# ivy-convert-airtable

Convert an Airtable base to an Ivy project.

## Step 1: Gather Credentials

You need an Airtable Base ID (e.g. `appXXXXXXXXXXXXXX`) and a Personal Access Token (PAT). Check if these were provided as arguments via `$ARGUMENTS`. If not, ask the user to provide them.

## Step 2: Research the Airtable Base

Use the `ivy-inspector-airtable` CLI tool to inspect the content of the Airtable base. This tool is already installed.

1. Run `ivy-inspector-airtable docs` to see all available commands and options.
2. Authentication: Pass `--pat <token>` before the command when using ivy-inspector-airtable.
3. Inspect the base using the Base ID and PAT. Issue multiple Bash commands in parallel to gather information as fast as possible.

Your goal is to answer these questions:
- What is the overall structure of the base?
- What tables exist and what are their purposes?
- What is the schema for each table (fields, types, relationships)?
- What views exist for each table and what filtering/sorting do they apply?
- Which tables represent **manageable data** (records the user would create, edit, delete -- e.g. contacts, orders, activities) vs **reference/lookup data** (static lists, configuration, categories)?
- What are the relationships between tables (linked records)?
- What field types are used and how should they map to database types?
- Any design/layout hints for how we can build a web-based application based on this base?

## Step 3: Present the Conversion Plan

Given the research output, present the user with a plan using the following structure:

```markdown
# Airtable to Ivy Conversion Plan

**Base ID:** [Base ID]

## Create Database

**Connection Name:** MyCompanyBase
**Namespace:** MyCompanyBase.Connections.MyCompanyBase

The base contains the following tables that should be converted to tables in a database:

### Table:Customers

**Airtable Table:** Customers
**Airtable Table ID:** tblXXXXXXXXXXXXXX
**Columns:**
- name: Name
  type: string
  nullable: false
  airtableType: singleLineText
  normalization: Trim(Value)
- name: Email
  type: string
  nullable: true
  airtableType: email
  normalization: Lowercase(Trim(Value))
- name: Status
  type: Enum
  nullable: false
  values: Active, Inactive, Pending
  airtableType: singleSelect
  normalization: <None>
- name: Tags
  type: string[]
  nullable: true
  airtableType: multipleSelects
  normalization: <None>
- name: AccountManager
  type: ForeignKey(Users)
  nullable: true
  airtableType: linkedRecord
  linkedTable: Users
  normalization: <None>
- ...

### Table:Orders

**Airtable Table:** Orders
**Airtable Table ID:** tblYYYYYYYYYYYYYY
**Columns:**
- name: OrderNumber
  type: string
  nullable: false
  airtableType: number/formula
  normalization: <None>
- name: Customer
  type: ForeignKey(Customers)
  nullable: false
  airtableType: linkedRecord
  linkedTable: Customers
  normalization: <None>
- name: OrderDate
  type: DateTime
  nullable: false
  airtableType: date
  normalization: <None>
- name: Total
  type: decimal
  nullable: false
  airtableType: currency
  normalization: <None>
- name: Attachments
  type: string[]
  nullable: true
  airtableType: multipleAttachments
  normalization: Store URLs or download to blob storage
- ...

## Apps

For each app, assign a **Type:**
- `CRUD` -- When the app is backed by a database table containing entity records that users would create, edit, and delete (e.g. contacts, orders, activities, deals). CRUD apps should include: DataTable with Add button in header, Sheet/Dialog for create/edit forms with validation, delete via row action with confirmation dialog.
- `Dashboard` -- For summary, analytics, or reporting views.
- `Ad-hoc` -- For utility or custom apps that don't fit the above categories.

Default to `CRUD` for any app backed by a database table with entity data. Only use read-only views when the user explicitly requests it or the data is reference/lookup data.

### App:Customers

**File:** /Apps/CustomersApp.cs
**Icon:** User
**Table:** Customers
**Type:** CRUD
**Description:**
Full CRUD management for customer records: DataTable with Add button, Sheet for create/edit forms with validation, delete via row action with confirmation. Include filters for Status and search by Name/Email.

Airtable Views to consider:
- "All Customers" (default view)
- "Active Only" (filtered by Status = Active)
- "Recently Added" (sorted by Created Time desc)

### App:Orders

**File:** /Apps/OrdersApp.cs
**Icon:** ShoppingCart
**Table:** Orders
**Type:** CRUD
**Description:**
Full CRUD management for order records: DataTable with Add button, Sheet for create/edit forms with validation, delete via row action with confirmation. Include Customer lookup/selector, date pickers, and currency formatting.

Airtable Views to consider:
- "All Orders" (default view)
- "Pending Orders" (filtered by Status)
- "By Customer" (grouped by Customer)

### App:Dashboard

**File:** /Apps/DashboardApp.cs
**Icon:** Dashboard
**Type:** Dashboard
**Description:**
Overview dashboard showing key metrics: total customers, active customers, total orders, revenue charts. Use BarChart, LineChart, and Stat widgets.

## Migration Notes

- **Attachments**: Airtable attachment URLs are temporary. Use the `export` command to download attachments locally before migration, or implement a migration script to upload them to blob storage.
- **Linked Records**: Airtable's linkedRecord fields should become foreign keys. Ensure referential integrity is maintained.
- **Formula Fields**: Airtable formulas should be evaluated and either converted to computed properties in C# or stored as static values during migration.
- **Rollup/Lookup Fields**: Convert to SQL views, computed properties, or aggregate queries.
- **Collaborator Fields**: Map to User table foreign keys if implementing user management.

## Other Notes

- Preserve view configurations (filters, sorts, groupings) as default UI states
- Consider implementing view presets that match Airtable views
- Migrate attachment files using `ivy-inspector-airtable export` command
- Test data migration with a subset before full migration
```

## Step 4: Implementation

Implement the plan approved by the user in the previous step.
