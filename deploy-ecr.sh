#!/bin/bash

# AWS ECR Docker Deploy Script for Visita Booking API
# Usage: ./deploy-ecr.sh [tag] [--push-only] [--build-only]

set -e  # Exit on any error

# Configuration
AWS_ACCOUNT_ID="766670502987"
AWS_REGION="ap-southeast-1"
REPOSITORY_NAME="visita-booking-api"
ECR_URI="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com/${REPOSITORY_NAME}"

# Default values
IMAGE_TAG="${1:-latest}"
BUILD_ONLY=false
PUSH_ONLY=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --build-only)
            BUILD_ONLY=true
            shift
            ;;
        --push-only)
            PUSH_ONLY=true
            shift
            ;;
        --help|-h)
            echo "Usage: $0 [tag] [options]"
            echo "Options:"
            echo "  --build-only    Only build the image, don't push"
            echo "  --push-only     Only push existing image, don't build"
            echo "  --help, -h      Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0                    # Build and push with 'latest' tag"
            echo "  $0 v1.2.3            # Build and push with 'v1.2.3' tag"
            echo "  $0 latest --build-only # Only build the image"
            echo "  $0 latest --push-only  # Only push existing image"
            exit 0
            ;;
        *)
            if [[ $1 != --* ]]; then
                IMAGE_TAG="$1"
            fi
            shift
            ;;
    esac
done

echo "🚀 AWS ECR Deployment Script"
echo "=============================="
echo "ECR URI: $ECR_URI"
echo "Image Tag: $IMAGE_TAG"
echo "Region: $AWS_REGION"
echo ""

# Check if AWS CLI is installed
if ! command -v aws &> /dev/null; then
    echo "❌ Error: AWS CLI is not installed. Please install it first."
    echo "Install guide: https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html"
    exit 1
fi

# Check if Docker is running
if ! docker info &> /dev/null; then
    echo "❌ Error: Docker is not running. Please start Docker first."
    exit 1
fi

# Check AWS credentials
if ! aws sts get-caller-identity &> /dev/null; then
    echo "❌ Error: AWS credentials not configured or invalid."
    echo "Please run 'aws configure' or set up your credentials."
    exit 1
fi

# Function to build Docker image
build_image() {
    echo "🔨 Building Docker image..."
    
    # Check if Dockerfile exists
    if [ ! -f "Dockerfile" ]; then
        echo "❌ Error: Dockerfile not found in current directory."
        exit 1
    fi
    
    # Build the image with multiple tags
    docker build \
        --platform linux/amd64 \
        -t "${REPOSITORY_NAME}:${IMAGE_TAG}" \
        -t "${REPOSITORY_NAME}:latest" \
        -t "${ECR_URI}:${IMAGE_TAG}" \
        -t "${ECR_URI}:latest" \
        .
    
    echo "✅ Docker image built successfully"
}

# Function to push to ECR
push_to_ecr() {
    echo "🔐 Authenticating with AWS ECR..."
    
    # Get ECR login token and authenticate Docker
    aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $ECR_URI
    
    if [ $? -eq 0 ]; then
        echo "✅ Successfully authenticated with ECR"
    else
        echo "❌ Error: Failed to authenticate with ECR"
        exit 1
    fi
    
    echo "⬆️  Pushing image to ECR..."
    
    # Push both the specific tag and latest
    docker push "${ECR_URI}:${IMAGE_TAG}"
    
    if [ "$IMAGE_TAG" != "latest" ]; then
        docker push "${ECR_URI}:latest"
    fi
    
    echo "✅ Successfully pushed to ECR"
}

# Function to show image information
show_image_info() {
    echo ""
    echo "📊 Image Information:"
    echo "===================="
    
    # Get image size
    IMAGE_SIZE=$(docker images "${ECR_URI}:${IMAGE_TAG}" --format "table {{.Size}}" | tail -n +2)
    echo "Image Size: $IMAGE_SIZE"
    
    # Get image ID
    IMAGE_ID=$(docker images "${ECR_URI}:${IMAGE_TAG}" --format "table {{.ID}}" | tail -n +2)
    echo "Image ID: $IMAGE_ID"
    
    echo "ECR URI: ${ECR_URI}:${IMAGE_TAG}"
}

# Function to verify ECR repository exists
verify_ecr_repo() {
    echo "🔍 Verifying ECR repository exists..."
    
    if aws ecr describe-repositories --repository-names $REPOSITORY_NAME --region $AWS_REGION &> /dev/null; then
        echo "✅ ECR repository exists"
    else
        echo "⚠️  ECR repository does not exist. Creating it..."
        aws ecr create-repository \
            --repository-name $REPOSITORY_NAME \
            --region $AWS_REGION \
            --image-scanning-configuration scanOnPush=true \
            --encryption-configuration encryptionType=AES256
        
        if [ $? -eq 0 ]; then
            echo "✅ ECR repository created successfully"
        else
            echo "❌ Error: Failed to create ECR repository"
            exit 1
        fi
    fi
}

# Function to cleanup old images (optional)
cleanup_old_images() {
    echo "🧹 Cleaning up old local images..."
    
    # Remove old untagged images
    OLD_IMAGES=$(docker images -f "dangling=true" -q)
    if [ ! -z "$OLD_IMAGES" ]; then
        docker rmi $OLD_IMAGES || true
        echo "✅ Cleaned up old untagged images"
    else
        echo "ℹ️  No old images to clean up"
    fi
}

# Main execution
main() {
    # Verify ECR repository exists
    verify_ecr_repo
    
    if [ "$PUSH_ONLY" = false ]; then
        build_image
    fi
    
    if [ "$BUILD_ONLY" = false ]; then
        push_to_ecr
    fi
    
    show_image_info
    cleanup_old_images
    
    echo ""
    echo "🎉 Deployment completed successfully!"
    echo ""
    echo "Next steps:"
    echo "- Update your ECS task definition or Kubernetes deployment"
    echo "- Use this image: ${ECR_URI}:${IMAGE_TAG}"
    echo "- Monitor your application logs after deployment"
}

# Run main function
main