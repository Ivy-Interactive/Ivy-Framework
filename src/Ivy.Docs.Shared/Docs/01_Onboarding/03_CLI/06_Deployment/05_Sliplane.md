---
searchHints:
  - deployment
  - cloud
  - production
  - sliplane
  - docker
---

# Sliplane Deployment

<Ingress>
**Sliplane** - Modern container deployment platform with automated infrastructure and simplified deployment workflow.
</Ingress>

## Setup Process

```terminal
>ivy deploy
# Select Sliplane when prompted
```

**Required Configuration** - Sliplane API Key, Server (optional, will be created if not specified), and Port Configuration (defaults to port 80).

**Sliplane Services Used** - Container hosting and deployment, automated SSL/TLS certificates, load balancing and traffic routing, and automated health checks and monitoring.

**Sliplane Setup Prerequisites** - Create a Sliplane account, generate an API key from your Sliplane dashboard, and optionally create a server in your Sliplane dashboard (or let Ivy create one automatically).

## Basic Sliplane Deployment

```terminal
>ivy deploy
# Select Sliplane
# Configure server and deployment settings
```

For detailed information about Sliplane cloud provider:

- **Sliplane**: [Sliplane Documentation](https://docs.sliplane.io/)