---
searchHints:
  - deployment
  - cloud
  - production
  - azure
  - docker
---

# Azure Deployment

<Ingress>
**Azure (Microsoft Azure)** - Cloud services for building, deploying, and managing applications.
</Ingress>

## Setup Process

```terminal
>ivy deploy
# Select Azure when prompted
```

**Required Configuration** - Azure Subscription, Resource Group, Container Registry (ACR), and Container Apps Environment.

**Azure Services Used** - Azure Container Registry, Azure Container Apps (serverless container platform), Azure Resource Manager, and Azure Active Directory.

**Azure Setup Prerequisites** - Create an Azure account, install Azure CLI, login to Azure: `az login`, and set your subscription: `az account set --subscription <subscription-id>`.

## Basic Azure Deployment

```terminal
>ivy deploy --verbose
# Select Azure
# Configure custom resource group and region
```

For detailed information about Azure cloud provider:

- **Azure**: [Azure Documentation](https://docs.microsoft.com/azure/)
