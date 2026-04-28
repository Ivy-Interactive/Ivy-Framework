---
name: ivy-create-graphql-connection
description: >
  Create a GraphQL API connection using StrawberryShake code generation. Use when
  the user wants to add a GraphQL connection, integrate a GraphQL API, set up
  StrawberryShake, or add GraphQL support to their Ivy project.
allowed-tools:
  - Bash(dotnet:*)
  - Read
  - Write
  - Edit
  - Glob
  - Grep
effort: high
---

# Create a GraphQL Connection

This skill creates a GraphQL API connection in an Ivy project using StrawberryShake for typed client generation. It researches the API, collects configuration, generates the connection files, and verifies the build.

## Pre-flight: Read Learnings

If the file `.ivy/learnings/ivy-create-graphql-connection.md` exists in the project directory, read it first and apply any lessons learned from previous runs of this skill.

## Prerequisites

- The working directory must be a valid Ivy project.
- The user should know (or you should research) the GraphQL endpoint URL and authentication method.

## Workflow

### 1. Research the API

If the GraphQL endpoint URL is not already known, research the API to find:

1. The GraphQL endpoint URL (usually ends in `/graphql`)
2. The authentication method (bearer token, API key header, query parameter)
3. Any specific header or parameter names for auth

Use web search and documentation to find this information.

If the endpoint URL is already known, skip to step 2.

### 2. Collect Inputs

Gather the following information from the user (ask the user for any values not already known):

1. **GraphQL Endpoint URL** -- must be a valid HTTP/HTTPS URL
2. **Connection Name** -- PascalCase name for the connection (e.g., `GitHubGraphQL`). Must match `^[A-Z][a-zA-Z0-9]*$` and not conflict with existing connections.
3. **Authentication Token** -- the API token or key (treat as secret)
4. **Auth Delivery Method** -- how the token is sent to the API. Options:
   - `bearer` -- Bearer token in the Authorization header
   - `queryParam` -- Query parameter (e.g., `?token=xxx`)
   - `customHeader` -- Custom header (e.g., `X-Api-Key`)

   The auth method may be auto-detected by probing the endpoint. If auto-detection fails, ask the user.
5. If `customHeader` is selected, ask for the **Auth Header Name** (default: `X-Api-Key`)
6. If `queryParam` is selected, ask for the **Auth Query Param Name** (default: `token`)

### 3. Generate the Connection

The generation service will:

- Create the connection folder at `Connections/[ConnectionName]/`
- Configure StrawberryShake for the GraphQL endpoint
- Store the endpoint URL in dotnet user secrets as `[ConnectionName]:EndpointUrl`
- Store the authentication token in dotnet user secrets as `[ConnectionName]:Token`

### 4. Build and Verify

Run `dotnet build` to trigger StrawberryShake code generation. The `I[ConnectionName]Client` interface does not exist until the first build completes.

After building, test the connection to verify it works. If the test fails, investigate and fix the issue.

### 5. Completion

Tell the user the connection is ready and show them how to use it. Remind them to edit the `.graphql` query files to define the queries they need.

The connection folder structure will be at `Connections/[ConnectionName]/`.

## Recovery Guidance

If the connection setup fails, follow these recovery steps:

1. **Diagnose the root cause** (e.g., invalid endpoint, auth issues, network problems, schema introspection failure):
   - For invalid endpoint: Verify the GraphQL endpoint URL is accessible and returns a valid response to `{ __typename }`
   - For auth issues: Ensure the authentication token is correct and the auth delivery method (header, query param) matches the API's requirements
   - For network problems: Check network connectivity and firewall settings
   - For schema introspection failure: Verify the endpoint supports introspection queries (some production APIs disable it)

2. **After fixing the underlying issue**, choose ONE of these approaches:

   **Option A: Retry the skill** (recommended for transient failures like network timeouts or temporary introspection issues)
   - Use the `/ivy-create-graphql-connection` skill to restart the connection setup from scratch.

   **Option B: Manual GraphQL connection creation** (recommended for persistent failures or APIs with introspection disabled)
   - Create a GraphQL client class in `Connections/[ConnectionName]/`
   - Manually define types based on the API documentation (if introspection is unavailable)
   - Configure the client with the endpoint URL and authentication method
   - Test the connection with a simple query

3. **Do NOT proceed** to generating apps or widgets until the connection is successfully established and tested with a sample query.

## Post-run: Evaluate and Improve

After completing the task:

1. **Evaluate**: Did the build succeed? Were there compilation errors, unexpected behavior, or manual corrections needed during this run?
2. **Update learnings**: If anything required correction or was surprising, append a concise entry to `.ivy/learnings/ivy-create-graphql-connection.md` (create the file and `.ivy/learnings/` directory if they don't exist). Each entry should note: the date, what went wrong, why, and what to do differently next time.
3. **Skip if clean**: If everything succeeded without issues, do not update the learnings file.
