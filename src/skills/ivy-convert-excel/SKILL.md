---
name: ivy-convert-excel
description: >
  Convert an Excel (.xlsx) document to an Ivy project. Use when the user wants to
  migrate from Excel, convert a spreadsheet, import Excel data, or build an Ivy
  app from an .xlsx file. Uses the ivy-inspector-excel CLI tool to inspect and
  extract data from Excel files.
allowed-tools: Bash(ivy-inspector-excel:*) Bash(dotnet:*) Bash(git:*) Bash(test:*) Bash(realpath:*) Bash(cygpath:*) Bash(mkdir:*) Read Write Edit Glob Grep
effort: high
argument-hint: "[path to .xlsx file]"
---

# ivy-convert-excel

Convert an Excel (.xlsx) document to an Ivy project.

## Step 1: Locate the Excel File

You need a path to a `.xlsx` file. Check if a path was provided as arguments via `$ARGUMENTS`. If not, ask the user to provide one.

1. Verify the file exists using `test -f "<path>"`.
2. Convert the path to an absolute path using `realpath "<path>"` (or `cygpath -w` on Windows if needed). Use this absolute path in all subsequent commands.

## Step 2: Research the Excel File

Use the `ivy-inspector-excel` CLI tool to inspect the content of the Excel file. This tool is already installed.

1. Run `ivy-inspector-excel docs` to see all available commands and options.
2. Inspect the xlsx file using ivy-inspector-excel. Issue multiple Bash commands in parallel to gather information as fast as possible.

Your goal is to answer these questions:
- What is the overall structure?
- Any design/layout hints for how we can build a web-based application based on this document?
- What terminology is used? What are the entities?
- What is the underlying logic of the sheet? What formulas are used?
- What are the relationships between the different sheets (if there are multiple sheets)?
- What tables or ranges are suitable to convert to tables in a relational database (if any)?
- Which entities represent **manageable data** (records the user would create, edit, delete -- e.g. contacts, orders, activities) vs **reference/lookup data** (static lists, configuration, categories)?

3. After inspecting the Excel file and identifying all data ranges suitable for conversion, extract the data:
   - Create the output directory: `mkdir -p .ivy/data`
   - For each identified table/range, extract the data to a CSV file:
     ```
     ivy-inspector-excel extract "<path-to-xlsx>" "<sheet-name>" "<range>" "<absolute-path-to/.ivy/data/table-name.csv>"
     ```
   - Use lowercase, hyphenated filenames matching the entity name (e.g. `customers.csv`, `order-items.csv`)
   - Include the list of extracted CSV files and their source sheet/range in your analysis

## Step 3: Present the Conversion Plan

Given the research output, present the user with a plan using the following structure:

```markdown
# Excel to Ivy Conversion Plan

**File:** [path to xlsx file]

## Create Database

**Connection Name:** MyCompanyCrm
**Namespace:** MyCompanyCrm.Connections.MyCompanyCrm

The document contains the following entities that should be converted to tables in a database:

### Table:Customers

**Sheet:** Sheet1
**Range:** A1:D90
**Columns:**
- name: name
  type: string
  nullable: false
  normalization: Trim(UpperCase(Value))
- name: Age
  type: int
  nullable: false
  default: 0
  normalisation: <None>
- name: Gender
  type: Enum
  nullable: false
  values: Male, Female, Other
  normalization: The source file has some misspelled values 'Femal' these should be mapped to "Female"
- ...

### Table:Orders

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
Full CRUD management for customer records: DataTable with Add button, Sheet for create/edit forms with validation, delete via row action with confirmation.

### App:Dashboard

**File:** /Apps/DashboardApp.cs
**Icon:** Dashboard
**Type:** Dashboard
**Description:**
...

### App:Quote

**File:** /Apps/QuoteApp.cs
**Icon:** Numbers
**Type:** CRUD
**Description:**
Full CRUD management for quote records: DataTable with Add button, Sheet for create/edit forms with validation, delete via row action with confirmation.

## Other Notes

...

```

## Step 4: Implementation

Implement the plan approved by the user in the previous step.
