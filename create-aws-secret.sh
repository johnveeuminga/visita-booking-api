#!/bin/bash

# Script to create AWS credentials secret for Kubernetes (using credentials file format)
# Usage: ./create-aws-secret.sh

echo "Creating AWS credentials secret for Kubernetes (credentials file format)..."
echo "Please provide your AWS credentials:"

read -p "AWS Access Key ID: " AWS_ACCESS_KEY_ID
read -s -p "AWS Secret Access Key: " AWS_SECRET_ACCESS_KEY
echo ""
read -p "AWS Region (default: ap-southeast-1): " AWS_REGION
AWS_REGION=${AWS_REGION:-ap-southeast-1}

# Validate inputs
if [ -z "$AWS_ACCESS_KEY_ID" ] || [ -z "$AWS_SECRET_ACCESS_KEY" ]; then
    echo "Error: Both AWS Access Key ID and Secret Access Key are required."
    exit 1
fi

# Create temporary credentials file
cat > /tmp/credentials << EOF
[default]
aws_access_key_id = $AWS_ACCESS_KEY_ID
aws_secret_access_key = $AWS_SECRET_ACCESS_KEY
EOF

# Create temporary config file
cat > /tmp/config << EOF
[default]
region = $AWS_REGION
output = json
EOF

# Create the secret with files
kubectl create secret generic aws-credentials \
    --from-file=credentials=/tmp/credentials \
    --from-file=config=/tmp/config \
    --dry-run=client -o yaml > aws-credentials-secret.yaml

# Clean up temporary files
rm -f /tmp/credentials /tmp/config

echo ""
echo "AWS credentials secret YAML has been generated: aws-credentials-secret.yaml"
echo "This secret contains AWS credentials and config files in the proper format."
echo ""
echo "Apply it to your cluster with: kubectl apply -f aws-credentials-secret.yaml"
echo ""
echo "The secret will be mounted to /app/aws/ in the container with:"
echo "  - /app/aws/credentials (contains access keys)"
echo "  - /app/aws/config (contains region and output format)"
echo ""
echo "Environment variables will point to these files:"
echo "  - AWS_SHARED_CREDENTIALS_FILE=/app/aws/credentials"
echo "  - AWS_CONFIG_FILE=/app/aws/config"
echo ""

# Optional: Apply directly if user wants
read -p "Do you want to apply the secret directly to the cluster now? (y/N): " APPLY_NOW
if [ "$APPLY_NOW" = "y" ] || [ "$APPLY_NOW" = "Y" ]; then
    kubectl apply -f aws-credentials-secret.yaml
    echo "Secret applied successfully!"
    echo ""
    echo "Verify the secret was created:"
    echo "kubectl get secret aws-credentials"
    echo ""
    echo "You can now deploy your application with:"
    echo "kubectl apply -f k8s-deployment.yaml"
    echo ""
    echo "After deployment, verify AWS credentials are working:"
    echo "./verify-aws-credentials.sh"
else
    echo "Secret YAML saved. Apply manually when ready."
fi