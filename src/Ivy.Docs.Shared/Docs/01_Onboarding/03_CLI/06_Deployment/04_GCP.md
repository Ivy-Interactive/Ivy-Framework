---
searchHints:
  - deployment
  - cloud
  - production
  - gcp
  - docker
---

# GCP Deployment

<Ingress>
**GCP (Google Cloud Platform)** - Cloud computing services for building, testing, and deploying applications.
</Ingress>

##Setup Process

```terminal
>ivy deploy
# Select GCP when prompted
```

**Required Configuration** - GCP Project, Container Registry (GCR), Cloud Run Service, and Region.

**GCP Services Used** - Google Container Registry, Cloud Run (serverless container platform), Cloud Build, and IAM.

**GCP Setup Prerequisites** - Create a Google Cloud account, install Google Cloud CLI, login to GCP: `gcloud auth login`, and set your project: `gcloud config set project <project-id>`.

## Basic GCP Deployment

```terminal
>ivy deploy
# Select GCP
# Configure project and region
```

For detailed information about GCP cloud provider:

- **GCP**: [Google Cloud Documentation](https://cloud.google.com/docs/)
