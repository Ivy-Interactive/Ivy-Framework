# Ivy Deployment

<Ingress>
Deploy your Ivy applications to cloud platforms with automated containerization, infrastructure setup, and configuration management.
</Ingress>

The `ivy deploy` command allows you to deploy your Ivy project to various cloud platforms. Ivy supports multiple deployment providers and automatically handles the deployment process including containerization, configuration, and infrastructure setup.

## Supported Deployment Providers

Ivy supports the following cloud deployment providers:

### Cloud Platforms

- **AWS** - Amazon Web Services
- **Azure** - Microsoft Azure
- **GCP** - Google Cloud Platform

## Basic Usage

### Deploying Your Project

```terminal
>ivy deploy
```

This command will:

- Prompt you to select a deployment provider
- Build and containerize your project
- Configure the necessary cloud resources
- Deploy your project to the selected platform
- Provide you with the deployment URL

### Command Options

`--project-path <PATH>` - Specify the path to your project directory. Defaults to the current directory.

```terminal
>ivy deploy --project-path /path/to/your/project
```

`--verbose` - Enable verbose output for detailed logging during deployment.

```terminal
>ivy deploy --verbose
```

### Interactive Mode

When you run `ivy deploy` without specifying options, Ivy will guide you through an interactive deployment process:

1. **Select Deployment Provider**: Choose from AWS, Azure, or GCP
2. **Configuration Setup**: Configure provider-specific settings
3. **Build Process**: Ivy will build and containerize your project
4. **Deployment**: Deploy to the selected cloud platform

### Deployment Provider Configuration

**AWS (Amazon Web Services)** - Comprehensive cloud platform with various services for application deployment.

**Setup Process**

```terminal
>ivy deploy
# Select AWS when prompted
```

**Required Configuration** - AWS Credentials (access key and secret key), Region, ECR Repository, and App Runner Service.

**AWS Services Used** - Amazon ECR (container registry), AWS App Runner (serverless container service), Amazon S3 (storage for build artifacts), and AWS IAM (identity and access management).

**AWS Setup Prerequisites** - Create an AWS account, install and configure AWS CLI, create an IAM user with appropriate permissions, and configure AWS credentials: `aws configure`.

**Azure (Microsoft Azure)** - Cloud services for building, deploying, and managing applications.

**Setup Process**

```terminal
>ivy deploy
# Select Azure when prompted
```

**Required Configuration** - Azure Subscription, Resource Group, Container Registry (ACR), and Container Apps Environment.

**Azure Services Used** - Azure Container Registry, Azure Container Apps (serverless container platform), Azure Resource Manager, and Azure Active Directory.

**Azure Setup Prerequisites** - Create an Azure account, install Azure CLI, login to Azure: `az login`, and set your subscription: `az account set --subscription <subscription-id>`.

**GCP (Google Cloud Platform)** - Cloud computing services for building, testing, and deploying applications.

**Setup Process**

```terminal
>ivy deploy
# Select GCP when prompted
```

**Required Configuration** - GCP Project, Container Registry (GCR), Cloud Run Service, and Region.

**GCP Services Used** - Google Container Registry, Cloud Run (serverless container platform), Cloud Build, and IAM.

**GCP Setup Prerequisites** - Create a Google Cloud account, install Google Cloud CLI, login to GCP: `gcloud auth login`, and set your project: `gcloud config set project <project-id>`.

### Deployment Process

**1. Project Validation** - Ivy validates your project before deployment: ensures it's an Ivy project, checks for required files and configuration, and validates authentication status.

**2. Build Process** - Ivy builds your project: restores NuGet packages, builds the project, creates Docker container, and pushes to container registry.

**3. Infrastructure Setup** - Ivy configures cloud resources: creates necessary cloud services, configures networking and security, and sets up monitoring and logging.

**4. Project Deployment** - Ivy deploys your project: deploys container to cloud platform, configures environment variables, sets up custom domains (if configured), and provides deployment URL.

### Containerization

**Docker Configuration** - Ivy automatically generates a `Dockerfile` for your project:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["YourProject.csproj", "./"]
RUN dotnet restore "YourProject.csproj"
COPY . .
RUN dotnet build "YourProject.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YourProject.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YourProject.dll"]
```

**Environment Configuration** - Ivy configures environment variables for your deployed application:

```text
### Database connection strings
ConnectionStrings__MyDatabase="your-connection-string"

### Authentication settings
AUTH0_DOMAIN="your-auth0-domain"
AUTH0_CLIENT_ID="your-client-id"

### Project settings
ASPNETCORE_ENVIRONMENT="Production"
```

### Security and Configuration

**Secrets Management** - Ivy handles secrets securely during deployment: connection strings stored as environment variables, authentication secrets configured securely, and API keys managed through cloud provider secrets.

**Network Security** - Ivy configures network security: automatic SSL/TLS configuration, appropriate firewall rules, and network isolation (where applicable).

### Monitoring and Logging

**Application Monitoring** - Ivy sets up monitoring for your deployed application: health checks, performance and resource metrics, centralized log collection, and automated alerting for issues.

**Cloud Provider Monitoring** - Each cloud provider offers specific monitoring tools: AWS CloudWatch, Azure Application Insights, and GCP Cloud Monitoring.

### Deployment Options

**Build Choices** - Ivy offers different build options:

**Local Build** - Builds container locally, requires Docker installed, and faster for development.

**Cloud Build** - Builds in cloud provider, no local Docker required, and consistent build environment.

**Scaling Configuration** - Configure application scaling: auto-scaling (automatic scaling based on demand), manual scaling (fixed number of instances), and min/max instances (scaling boundaries).

### Troubleshooting

**Common Deployment Issues**

**Build Failures** - Check that project builds locally, verify all dependencies are included, and check for compilation errors.

**Container Registry Issues** - Verify container registry credentials, check network connectivity, and ensure proper permissions.

**Cloud Provider Issues** - Verify cloud provider credentials, check resource quotas and limits, and ensure required services are enabled.

**Debugging Deployment**

**Enable Verbose Logging**

```terminal
>ivy deploy --verbose
```

**Check Cloud Provider Logs** - AWS CloudWatch logs, Azure Application Insights, and GCP Cloud Logging.

**Verify Deployment Status**

```bash
# Check deployment status in cloud console
# Verify application is accessible
# Check environment variables and configuration
```

## Examples

**Basic AWS Deployment**

```terminal
>ivy deploy
# Select AWS
# Follow prompts for configuration
```

**Azure Deployment with Custom Settings**

```terminal
>ivy deploy --verbose
# Select Azure
# Configure custom resource group and region
```

**GCP Deployment**

```terminal
>ivy deploy
# Select GCP
# Configure project and region
```

### Best Practices

**Pre-deployment Checklist** - Test locally (ensure your project runs locally), check dependencies (verify all required services are configured), review configuration (check environment variables and settings), and security review (verify authentication and authorization setup).

**Deployment Strategy** - Blue-green deployment (zero-downtime deployments), rolling updates (gradual deployment across instances), and canary deployments (test with subset of users).

**Cost Optimization** - Right-sizing (choose appropriate instance sizes), auto-scaling (configure scaling based on actual usage), resource cleanup (remove unused resources), and monitoring (track resource usage and costs).

**Security Best Practices** - Secrets management (use cloud provider secrets services), network security (configure appropriate firewall rules), access control (implement least-privilege access), and regular updates (keep dependencies and runtime updated).

### Post-deployment

**Application Management** - After deployment, you can monitor performance (use cloud provider monitoring tools), scale application (adjust instance count based on demand), update application (deploy new versions), and configure domains (set up custom domains and SSL).

**Maintenance** - Regular maintenance tasks include security updates (keep dependencies updated), performance monitoring (monitor and optimize performance), cost monitoring (track and optimize costs), and backup management (configure and test backups).

### Related Commands

- `ivy init` - Initialize a new Ivy project
- `ivy db add` - Add database connections
- `ivy auth add` - Add authentication providers
- `ivy app create` - Create apps

### Cloud Provider Documentation

For detailed information about each cloud provider:

- **AWS**: [AWS Documentation](https://docs.aws.amazon.com/)
- **Azure**: [Azure Documentation](https://docs.microsoft.com/azure/)
- **GCP**: [Google Cloud Documentation](https://cloud.google.com/docs/)