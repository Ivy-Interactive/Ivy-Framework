---
name: ivy-convert-lovable
description: >
  Convert a Lovable (lovable.dev) application to an Ivy project. Use when the user
  wants to migrate from Lovable, convert a Lovable app, or build an Ivy app from a
  Lovable project. Lovable generates React+Vite+TypeScript apps with Supabase backends.
  Handles GitHub URLs and local paths.
allowed-tools: Bash(dotnet:*) Bash(git:*) Bash(test:*) Bash(mkdir:*) Read Write Edit Glob Grep
effort: high
argument-hint: "[GitHub URL or local path]"
---

# ivy-convert-lovable

Convert a Lovable (lovable.dev) application to an Ivy project. Lovable generates React+Vite+TypeScript apps with Supabase backends.

## Pre-flight: Read Learnings

If the file `.ivy/learnings/ivy-convert-lovable.md` exists in the project directory, read it first and apply any lessons learned from previous runs of this skill.

## Reference Files

Read before implementing:
- [references/AGENTS.md](references/AGENTS.md) -- Ivy framework API reference (widgets, hooks, layouts, inputs, colors)

## Reference Files

The [references/](references/) folder contains 31 reference files with React/shadcn-to-Ivy component mappings. Read the relevant reference files before implementing the conversion to understand how to map Lovable/React features to Ivy features.

## Step 1: Locate the Lovable Project

You need a path to a cloned Lovable GitHub repository or a GitHub URL. Check if a value was provided via `$ARGUMENTS`. If not, ask the user to provide one.

- If it is a **GitHub URL** (starts with `https://github.com/`): Clone it to `.ivy/source/<repo-name>/` using `git clone <url> .ivy/source/<repo-name>/` and use that as the path going forward.
- If it is a **local path**: Use it directly.
- Verify the path exists with `test -d "<path>"` and is a valid project with `test -f "<path>/package.json"`.

## Step 2: Verify it is a Lovable Project

Check for these identifying markers:
- `package.json` with Vite+React+TypeScript setup (often `"name": "vite_react_shadcn_ts"`)
- `src/integrations/supabase/client.ts` (auto-generated Supabase client)
- `src/integrations/supabase/types.ts` (auto-generated DB types)
- `supabase/config.toml` (Supabase project config)
- `components.json` (shadcn/ui config)
- `public/lovable-uploads/` directory (Lovable-specific assets)

If none of these markers are present, report that this does not appear to be a Lovable project and suggest verifying the path.

## Step 3: Research -- Extract Database Schema

Read these files in priority order:
1. `src/integrations/supabase/types.ts` -- The most complete source. Contains a typed `Database` object with `Tables`, `Views`, `Functions`, `Enums` for each schema. Each table has `Row`, `Insert`, `Update` types with all columns and their TypeScript types.
2. `supabase/migrations/*.sql` -- Authoritative SQL schema history. Migration filenames follow pattern `YYYYMMDDHHMMSS_<description>.sql`. Parse for `CREATE TABLE`, `ALTER TABLE`, `CREATE TYPE`, `CREATE POLICY`, `CREATE FUNCTION`, `CREATE TRIGGER`, `CREATE INDEX`.
3. `supabase/config.toml` -- Contains `project_id` and edge function declarations with `verify_jwt` settings.

For each table, document: name, columns with types, nullable status, defaults, primary keys, foreign keys, and any enums.

## Step 4: Research -- Extract Edge Functions

Read `supabase/functions/*/index.ts` for each edge function:
- Functions are Deno/TypeScript using `serve()` from `https://deno.land/std@0.168.0/http/server.ts`
- Access env vars via `Deno.env.get("SECRET_NAME")`
- Common patterns: CORS handling, JWT verification, external API calls (Stripe, OpenAI, ElevenLabs, Resend)
- Edge function declarations in `supabase/config.toml` under `[functions.<name>]` with `verify_jwt` settings
- For each function, document: name, purpose, HTTP methods, auth requirements, external API dependencies, env vars needed

## Step 5: Research -- Extract Frontend Structure

Read these files to understand the app structure:
1. `src/App.tsx` -- Route definitions mapping paths to page components
2. `src/pages/*.tsx` -- Each file is a route/page
3. `src/hooks/*.ts(x)` -- Custom React hooks, often for Supabase data fetching
4. `src/components/**/*.tsx` -- Feature-organized components (skip `src/components/ui/*.tsx` -- those are shadcn/ui primitives that map to Ivy widgets)
5. `src/lib/*.ts`, `src/utils/*.ts` -- Utility functions
6. `src/types/*.ts` -- Custom type definitions (if present)

## Step 6: Research -- Identify Data Sources and Connections

- Supabase tables from types.ts -> Convert to Ivy database Connection
- External APIs called from edge functions -> Note for manual setup
- Auth patterns (Supabase Auth) -> Map to Ivy auth patterns
- Supabase Realtime subscriptions -> Note for Ivy real-time handling
- Supabase Storage usage -> Note for Ivy file storage

## Step 7: Research -- Map Pages to Ivy Apps

- Each meaningful page becomes an Ivy App
- Landing/marketing pages may be skipped or simplified
- Auth pages are handled by Ivy's built-in auth
- CRUD pages -> DataTable + Sheet/Dialog pattern
- Dashboard pages -> Chart widgets + KPI cards
- Form pages -> Form widgets

## Step 8: Present the Conversion Plan

Given the research output, present the user with a plan using the following structure:

```markdown
# Lovable to Ivy Conversion Plan

**Source:** [path to Lovable project]

## Database

**Connection Name:** <derived from project>
**Namespace:** <ProjectName>.Connections.<ConnectionName>

The Supabase schema contains the following tables that should be converted to a database:

### Table:<TableName>

**Columns:**
- name: <column>
  type: <CSharp type>
  nullable: <bool>
  default: <default value if any>

### Table:<TableName2>

...

## Edge Functions -> Backend Logic

### Function:<FunctionName>

**Purpose:** <description>
**External APIs:** <list>
**Env Vars:** <list>
**Conversion:** <how to handle in Ivy - server action, scheduled job, webhook handler, etc.>

## Apps

For each app, assign a **Type:**
- `CRUD` -- When the app is backed by a database table containing entity records that users would create, edit, and delete (e.g. contacts, orders, activities, deals). CRUD apps should include: DataTable with Add button in header, Sheet/Dialog for create/edit forms with validation, delete via row action with confirmation dialog.
- `Dashboard` -- For summary, analytics, or reporting views.
- `Ad-hoc` -- For utility or custom apps that don't fit the above categories.

Default to `CRUD` for any app backed by a database table with entity data. Only use read-only views when the user explicitly requests it or the data is reference/lookup data.

### App:<AppName>

**File:** /Apps/<AppName>App.cs
**Icon:** <icon>
**Type:** CRUD | Dashboard | Ad-hoc
**Source Page:** src/pages/<Page>.tsx
**Description:** <what it does and how to convert>

### App:<AppName2>

...

## Auth

<auth pattern used in Lovable (Supabase Auth) and how to map to Ivy auth>

## Other Notes

<anything that can't be automatically converted, external API dependencies, env vars needed, etc.>
```

## Step 9: Implementation

Identify if there are any connections (db, auth, api) that should be set up using the appropriate connection skill (e.g., `/ivy-create-db-connection` for databases, `/ivy-create-auth-connection` for auth, `/ivy-create-any-connection` for APIs).

Given the conversion plan from the previous step, implement the conversion of the Lovable application to an Ivy application. Use the conversion plan and the reference files as guides to map Lovable/React features to Ivy features.

The conversion guide should include:
1. **Database schema** -- All tables with columns, types mapped to C#, relationships
2. **Edge functions** -- Purpose, external dependencies, how to handle in Ivy
3. **Apps** -- Each page mapped to an Ivy App with type (CRUD/Dashboard/Ad-hoc), widgets needed, and data sources
4. **Auth** -- How Supabase Auth maps to Ivy auth
5. **Component mapping** -- Which React/shadcn components are used and their Ivy equivalents
6. **Hooks and state** -- How React hooks and state management map to Ivy patterns

## Post-run: Evaluate and Improve

After completing the task:

1. **Evaluate**: Did the build succeed? Were there compilation errors, unexpected behavior, or manual corrections needed during this run?
2. **Update learnings**: If anything required correction or was surprising, append a concise entry to `.ivy/learnings/ivy-convert-lovable.md` (create the file and `.ivy/learnings/` directory if they don't exist). Each entry should note: the date, what went wrong, why, and what to do differently next time.
3. **Skip if clean**: If everything succeeded without issues, do not update the learnings file.
