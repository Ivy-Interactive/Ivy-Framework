---
name: ivy-create-openapi-connection
description: >
  Create an OpenAPI/REST API connection using Refitter code generation. Use when
  the user wants to add an OpenAPI connection, connect to a REST API, integrate a
  Swagger spec, set up Refitter, or add OpenAPI or REST support to their Ivy project.
allowed-tools:
  - Bash(dotnet:*)
  - Bash(refitter:*)
  - Read
  - Write
  - Edit
  - Glob
  - Grep
effort: high
---

# Create an OpenAPI Connection

This skill creates an OpenAPI/REST API connection in an Ivy project using Refitter for typed client generation. It collects the spec URL, analyzes the spec for size, handles large specs with filtering or pivoting, and generates the connection.

## Pre-flight: Read Learnings

If the file `.ivy/learnings/ivy-create-openapi-connection.md` exists in the project directory, read it first and apply any lessons learned from previous runs of this skill.

## Reference Files

Read before implementing:
- [references/AGENTS.md](references/AGENTS.md) -- Ivy framework API reference (widgets, hooks, layouts, inputs, colors)

## Prerequisites

- The working directory must be a valid Ivy project.
- The user should know (or you should find) the OpenAPI specification URL.

## Workflow

### 1. Collect Inputs

Gather the following information from the user (ask the user for any values not already known):

1. **OpenAPI Spec URL** -- must be a valid HTTP/HTTPS URL pointing to an OpenAPI/Swagger specification (JSON or YAML)

   If the spec cannot be fetched or parsed, the URL will be rejected with an error message. Ask the user for a corrected URL.

2. **Auth Scheme** -- automatically detected from the OpenAPI spec's security schemes. If detection fails, defaults to `bearer` with `Authorization` header.

3. **Connection Name** -- PascalCase name for the connection (e.g., `StripeApi`). Must match `^[A-Z][a-zA-Z0-9]*$` and not conflict with existing connections. A name is suggested based on the spec URL.

4. **API Endpoint URL** -- the base URL for API calls. Suggested from the spec's server/host configuration.

5. **Auth Credentials** -- Bearer token or API key depending on the detected auth scheme (treat as secret).

### 2. Analyze the Spec

After inputs are collected, the OpenAPI spec is analyzed for size and endpoint count. If analysis fails, proceed to generation anyway (Refitter will report its own errors).

### 3. Handle Large Specs

If the spec is oversized (very large file size or many endpoints), you must choose one of the following strategies:

**Warning: Very large OpenAPI specifications** (many KB, hundreds of endpoints) are likely to produce thousands of types, build errors from name conflicts, and may crash the session.

The available endpoints from the spec will be listed. Choose one of:

1. **FilterEndpoints** -- Select only the endpoints needed. Provide an array of path regex patterns (e.g., `["/v1/chat/.*", "/v1/models"]`) matching the listed paths. Refitter uses regex matching on these patterns. This is preferred when only a small number of endpoints are needed.

2. **ProceedFull** -- Generate a client for the full spec anyway (not recommended for specs this large).

3. **PivotToAdHoc** -- Abandon the OpenAPI approach and create a lightweight ad-hoc HTTP connection instead (recommended when you only need a few endpoints). This will switch to a different connection workflow.

If only a small number of endpoints are needed for the user's task, prefer FilterEndpoints or PivotToAdHoc.

If the spec is not oversized, skip directly to generation.

### 4. Generate the Connection

The Refitter code generation service will:

- Create the connection folder at `Connections/[ConnectionName]/`
- Generate typed C# client interfaces and models from the OpenAPI spec
- Apply endpoint filters if specified
- Store the endpoint URL in dotnet user secrets as `[ConnectionName]:EndpointUrl`
- Store the authentication token in dotnet user secrets

### 5. Build and Verify

After generation, build the project and test the connection. If the test fails, investigate and fix the issue.

### 6. Completion

Tell the user the connection is ready and show them how to use it.

The connection folder structure will be at `Connections/[ConnectionName]/`.

## Recovery Guidance

If the connection setup fails, follow these recovery steps:

1. **Diagnose the root cause** (e.g., invalid spec URL/path, malformed OpenAPI spec, codegen failure, auth config issues):
   - For invalid spec location: Verify the OpenAPI spec URL is accessible or the file path exists
   - For malformed spec: Validate the spec using an OpenAPI validator (https://editor.swagger.io/)
   - For codegen failure: Check for unsupported OpenAPI features or version mismatches
   - For auth issues: Verify authentication configuration matches the spec's security schemes

2. **After fixing the underlying issue**, choose ONE of these approaches:

   **Option A: Retry the skill** (recommended for transient failures like network timeouts or temporary spec URL issues)
   - Use the `/ivy-create-openapi-connection` skill to restart the connection setup from scratch.

   **Option B: Manual OpenAPI connection creation** (recommended for persistent failures like codegen issues)
   - Manually create client classes in `Connections/[ConnectionName]/`
   - Implement methods for the required API endpoints based on the spec
   - Configure authentication (API keys, OAuth, etc.) based on the spec's security schemes
   - Test the connection with a sample API call

3. **Do NOT proceed** to generating apps or widgets until the connection is successfully established and tested with a sample API call.

## Post-run: Evaluate and Improve

After completing the task:

1. **Evaluate**: Did the build succeed? Were there compilation errors, unexpected behavior, or manual corrections needed during this run?
2. **Update learnings**: If anything required correction or was surprising, append a concise entry to `.ivy/learnings/ivy-create-openapi-connection.md` (create the file and `.ivy/learnings/` directory if they don't exist). Each entry should note: the date, what went wrong, why, and what to do differently next time.
3. **Skip if clean**: If everything succeeded without issues, do not update the learnings file.
