---
searchHints:
  - deployment
  - cloud
  - production
  - aws
  - docker
---

# AWS Deployment

<Ingress>
**AWS (Amazon Web Services)** - Comprehensive cloud platform with various services for application deployment.
</Ingress>

## Setup Process

```terminal
>ivy deploy
# Select AWS when prompted
```

**Required Configuration** - AWS Credentials (access key and secret key), Region, ECR Repository, and App Runner Service.

**AWS Services Used** - Amazon ECR (container registry), AWS App Runner (serverless container service), Amazon S3 (storage for build artifacts), and AWS IAM (identity and access management).

**AWS Setup Prerequisites** - Create an AWS account, install and configure AWS CLI, create an IAM user with appropriate permissions, and configure AWS credentials: `aws configure`.

## Basic AWS Deployment

```terminal
>ivy deploy
# Select AWS
# Follow prompts for configuration
```

For detailed information about AWS cloud provider:

- **AWS**: [AWS Documentation](https://docs.aws.amazon.com/)