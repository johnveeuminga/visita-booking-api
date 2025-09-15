# AWS ECR Deployment Guide

This guide explains how to deploy the Visita Booking API to AWS ECR using the provided deployment script.

## Prerequisites

1. **AWS CLI installed and configured**
   ```bash
   # Install AWS CLI (if not already installed)
   curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
   unzip awscliv2.zip
   sudo ./aws/install

   # Configure AWS credentials
   aws configure
   ```

2. **Docker installed and running**
   ```bash
   # Check if Docker is running
   docker info
   ```

3. **Proper AWS permissions**
   - ECR repository access
   - Push/pull permissions

## Usage

### Basic Deployment (with latest tag)
```bash
./deploy-ecr.sh
```
This will:
- Build the Docker image
- Tag it as `latest`
- Push to ECR: `766670502987.dkr.ecr.ap-southeast-1.amazonaws.com/visita-booking-api:latest`

### Deploy with Custom Tag
```bash
./deploy-ecr.sh v1.2.3
```

### Build Only (don't push)
```bash
./deploy-ecr.sh --build-only
./deploy-ecr.sh v1.2.3 --build-only
```

### Push Only (don't build)
```bash
./deploy-ecr.sh --push-only
./deploy-ecr.sh v1.2.3 --push-only
```

## ECR Repository Details

- **Account ID**: 766670502987
- **Region**: ap-southeast-1
- **Repository**: visita-booking-api
- **Full URI**: 766670502987.dkr.ecr.ap-southeast-1.amazonaws.com/visita-booking-api

## Script Features

### Automatic Repository Creation
The script will automatically create the ECR repository if it doesn't exist.

### Security Features
- Image scanning enabled on push
- AES256 encryption
- Multi-architecture support (linux/amd64)

### Image Tags
Every deployment creates two tags:
- The specified tag (default: `latest`)
- The `latest` tag (always updated)

### Cleanup
The script automatically cleans up old dangling images after deployment.

## Troubleshooting

### Authentication Issues
```bash
# Re-authenticate with AWS
aws configure
aws sts get-caller-identity  # Verify credentials
```

### Docker Issues
```bash
# Restart Docker
sudo systemctl restart docker

# Check Docker status
docker info
```

### ECR Permission Issues
Ensure your AWS user/role has these permissions:
- `ecr:GetAuthorizationToken`
- `ecr:BatchCheckLayerAvailability`
- `ecr:GetDownloadUrlForLayer`
- `ecr:BatchGetImage`
- `ecr:InitiateLayerUpload`
- `ecr:UploadLayerPart`
- `ecr:CompleteLayerUpload`
- `ecr:PutImage`

## Example Output

```bash
🚀 AWS ECR Deployment Script
==============================
ECR URI: 766670502987.dkr.ecr.ap-southeast-1.amazonaws.com/visita-booking-api
Image Tag: latest
Region: ap-southeast-1

🔍 Verifying ECR repository exists...
✅ ECR repository exists
🔨 Building Docker image...
✅ Docker image built successfully
🔐 Authenticating with AWS ECR...
✅ Successfully authenticated with ECR
⬆️  Pushing image to ECR...
✅ Successfully pushed to ECR

📊 Image Information:
====================
Image Size: 245MB
Image ID: sha256:abc123...
ECR URI: 766670502987.dkr.ecr.ap-southeast-1.amazonaws.com/visita-booking-api:latest

🎉 Deployment completed successfully!
```

## Next Steps After Deployment

1. **Update ECS Task Definition** (if using ECS)
2. **Update Kubernetes Deployment** (if using EKS)
3. **Update Environment Variables** in your deployment configuration
4. **Monitor Application Logs** after deployment
5. **Run Health Checks** to verify deployment success