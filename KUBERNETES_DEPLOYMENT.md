# Kubernetes Deployment Guide

This guide explains how to deploy the Visita Booking API to Kubernetes using the provided templates.

## Prerequisites

1. **Kubernetes cluster** (EKS, GKE, or on-premise)
2. **kubectl** configured to connect to your cluster
3. **AWS CLI** configured with appropriate permissions
4. **Nginx Ingress Controller** installed in your cluster
5. **TLS certificate** for api-booking.baguio.visita.ph

## Quick Deployment Steps

### 1. Clone and Configure Templates

```bash
# Copy the deployment template
cp k8s-deployment.template.yaml k8s-deployment.yaml

# Copy AWS credentials templates  
cp aws-credentials.template aws-credentials
cp aws-config.template aws-config
```

### 2. Update Configuration Values

Edit `k8s-deployment.yaml` and replace these placeholders:

#### Database Configuration
- `REPLACE_WITH_YOUR_DATABASE_CONNECTION_STRING`
  ```
  Server=your-db-server;Database=visita_booking;User Id=username;Password=password;
  ```

#### JWT Configuration
- `REPLACE_WITH_YOUR_JWT_SECRET_KEY` - A secure random string (256-bit recommended)

#### Xendit Payment Configuration
- `REPLACE_WITH_YOUR_XENDIT_SECRET_KEY`
- `REPLACE_WITH_YOUR_XENDIT_WEBHOOK_TOKEN`

#### AWS Configuration
- `REPLACE_WITH_YOUR_S3_BUCKET_NAME`

#### SSL Certificate
- `REPLACE_WITH_YOUR_SSL_CERTIFICATE_ARN` - Your AWS ACM certificate ARN

### 3. Configure AWS Credentials

Edit `aws-credentials`:
```ini
[default]
aws_access_key_id = YOUR_ACTUAL_ACCESS_KEY
aws_secret_access_key = YOUR_ACTUAL_SECRET_KEY
region = ap-southeast-1
```

### 4. Create Kubernetes Secrets

```bash
# Create AWS credentials secret
kubectl create secret generic aws-credentials \
  --from-file=credentials=./aws-credentials \
  --from-file=config=./aws-config

# Create ECR registry secret for image pulling
AWS_ACCOUNT_ID="766670502987"
AWS_REGION="ap-southeast-1"
ECR_REGISTRY="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"
TOKEN=$(aws ecr get-login-password --region ${AWS_REGION})

kubectl create secret docker-registry ecr-registry-secret \
  --docker-server=${ECR_REGISTRY} \
  --docker-username=AWS \
  --docker-password=${TOKEN} \
  --docker-email=your-email@example.com

# Create TLS certificate secret (replace with your actual certificate files)
kubectl create secret tls booking-api-tls \
  --cert=path/to/your/certificate.crt \
  --key=path/to/your/private.key

# Or if using Let's Encrypt with cert-manager, it will be created automatically
```

### 5. Deploy to Kubernetes

```bash
# Apply the deployment
kubectl apply -f k8s-deployment.yaml

# Check deployment status
kubectl get deployments
kubectl get pods
kubectl get services
kubectl get ingress
```

### 6. Verify Deployment

```bash
# Check pod logs
kubectl logs -l app=visita-booking-api

# Check service endpoints
kubectl get endpoints

# Test health endpoint
kubectl port-forward svc/visita-booking-api-service 8080:80
curl http://localhost:8080/api/auth/health
```

## Configuration Details

### Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Production` |
| `ConnectionStrings__DefaultConnection` | Database connection | `Server=...` |
| `JWT__SecretKey` | JWT signing key | `your-secret-key` |
| `Xendit__SecretKey` | Xendit API key | `xnd_development_...` |
| `AWS_S3_BUCKET` | S3 bucket name | `visita-assets` |

### Resource Limits

- **CPU**: 250m (request) / 500m (limit)
- **Memory**: 512Mi (request) / 1Gi (limit)
- **Replicas**: 2 (with rolling updates)

### Health Checks

- **Liveness Probe**: `/api/auth/health` every 30s
- **Readiness Probe**: `/api/auth/health` every 10s

### Ingress Configuration

- **Domain**: `api-booking.baguio.visita.ph`
- **Ingress Controller**: Nginx
- **TLS**: Enabled with `booking-api-tls` secret
- **Path Type**: Prefix matching for all paths (`/`)

## Security Considerations

1. **Secrets Management**: All sensitive data stored in Kubernetes secrets
2. **Non-root Container**: Application runs as user ID 1000
3. **Read-only Root**: Minimizes attack surface
4. **Resource Limits**: Prevents resource exhaustion
5. **Health Checks**: Ensures application availability

## Troubleshooting

### Pod Not Starting
```bash
kubectl describe pod <pod-name>
kubectl logs <pod-name>
```

### Service Not Accessible
```bash
kubectl get endpoints
kubectl describe service visita-booking-api-service
```

### Ingress Issues
```bash
kubectl describe ingress visita-booking-api-ingress
kubectl get events --sort-by='.metadata.creationTimestamp'
```

### ECR Image Pull Issues
```bash
# Refresh ECR token (expires every 12 hours)
TOKEN=$(aws ecr get-login-password --region ap-southeast-1)
kubectl create secret docker-registry ecr-registry-secret \
  --docker-server=766670502987.dkr.ecr.ap-southeast-1.amazonaws.com \
  --docker-username=AWS \
  --docker-password=${TOKEN} \
  --docker-email=your-email@example.com \
  --dry-run=client -o yaml | kubectl apply -f -
```

## Monitoring and Scaling

### Horizontal Pod Autoscaler
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: visita-booking-api-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: visita-booking-api
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

### Update Deployment
```bash
# Update image tag
kubectl set image deployment/visita-booking-api visita-booking-api=766670502987.dkr.ecr.ap-southeast-1.amazonaws.com/visita-booking-api:v1.2.3

# Rollback if needed
kubectl rollout undo deployment/visita-booking-api
```