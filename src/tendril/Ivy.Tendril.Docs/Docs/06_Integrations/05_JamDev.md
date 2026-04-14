---
icon: Bug
searchHints:
  - jam
  - jam.dev
  - webhook
  - inbox api
  - bug reports
---

# jam.dev

<Ingress>
Integrate jam.dev with Tendril to automatically create plans from bug reports via the inbox API webhook.
</Ingress>

## Overview

jam.dev can send bug reports to Tendril's inbox API endpoint, which automatically creates plans via the MakePlan promptware.

## Webhook URL

Configure jam.dev to POST to:

```
http://localhost:5000/api/inbox
```

Replace `localhost:5000` with your Tendril host and port if configured differently.

## Request Format

Send a POST request with a JSON body:

```json
{
  "description": "Bug description from jam.dev",
  "project": "ProjectName",
  "sourcePath": "optional/path/to/related/code"
}
```

| Field | Required | Description |
|-------|----------|-------------|
| `description` | Yes | The bug report or issue description |
| `project` | No | Target project name (defaults to `Auto`) |
| `sourcePath` | No | Path hint for related source code |

## Authentication

If you have an API key configured in `config.yaml` under `api.apiKey`, include it in the request header:

```
X-Api-Key: your-api-key
```

<Callout type="tip">
Without an API key configured, the endpoint accepts unauthenticated requests. Set an API key in production environments.
</Callout>

## Response

A successful request returns:

```json
{
  "jobId": "abc123",
  "status": "Started",
  "message": "Plan creation started for project 'ProjectName'"
}
```

## Setting Up in jam.dev

1. Open your jam.dev workspace settings
2. Navigate to integrations or webhooks
3. Add a new webhook pointing to your Tendril inbox URL
4. Configure the payload to match the request format above
