# Telemetry Data Classification Policy

## Purpose

This document defines what data Tendril may and may not send to third-party telemetry services (PostHog). The goal is to collect useful analytics while respecting user privacy.

## Classification Rules

### ALLOWED - Aggregate & Non-Identifying Data

**Safe to track:**
- **Counts**: Number of projects, plans, jobs (aggregate totals only)
- **Durations**: Time taken to complete operations (in seconds)
- **States/Types**: Enum values, state names, job types (e.g., "MakePlan", "ExecutePlan")
- **Levels**: Plan levels (e.g., "Bug", "Critical", "NiceToHave")
- **Versions**: Application version strings
- **Booleans**: Feature flags, configuration states (e.g., llm_configured: true)
- **Status codes**: Success/failure indicators, verification results

**Why these are safe:**
- They cannot identify individual users, repositories, or organizations
- They provide useful aggregate analytics (usage patterns, performance metrics)
- They respect the principle of data minimization

### FORBIDDEN - Identifying Information

**Never track:**
- **URLs**: Repository URLs, PR URLs, issue URLs
- **Paths**: File paths, directory paths, absolute paths to repos
- **Usernames**: GitHub usernames, organization names, email addresses
- **Repository names**: Specific repository identifiers
- **Project names**: Even generic names from config.yaml (e.g., "Tendril", "Framework") reveal work context
- **Sequential IDs**: Plan IDs, issue numbers, PR numbers that could correlate users
- **User input**: Task descriptions, commit messages, plan content
- **Titles**: Plan titles, issue titles, commit subjects

**Why these are forbidden:**
- They can reveal private repository information
- They may expose personal or organizational identifiers
- Project names reveal what the user is working on, which could be sensitive business information
- Sequential IDs can be used to correlate activity across anonymous users
- User-provided content may contain sensitive or proprietary information

## Decision Framework

When adding new telemetry events, ask:

1. **Can this field identify a person or organization?** -> Forbidden
2. **Can this field reveal private repository information?** -> Forbidden
3. **Can this field reveal what the user is working on?** -> Forbidden
4. **Can this field be correlated across users to de-anonymize them?** -> Forbidden
5. **Does this field provide useful aggregate insights?** -> Allowed

**When in doubt, leave it out.**

## Current Events Audit

All events comply with this policy as of Plan 02085:

| Event | Status | Notes |
|-------|--------|-------|
| app_started | Compliant | Aggregate counts only |
| plan_created | Compliant | Project name removed |
| pr_created | Compliant | Project name removed |
| job_completed | Compliant | Job types + status enums |
| plan_state_transition | Compliant | Plan ID removed |

## Implementation

See [ITelemetryService.cs](Services/ITelemetryService.cs) for typed context objects that enforce this policy at compile time.

## History

- 2026-04-06: Plan 02069 removed repo_url from pr_created event
- 2026-04-06: Plan 02085 established this policy document and removed project names and plan IDs
