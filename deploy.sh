#!/bin/bash

# Sweepo Server Docker Deployment Script
# This script builds and deploys the Sweepo server in a Docker container

set -e  # Exit on any error

# Configuration
CONTAINER_NAME="sweepo-server"
IMAGE_NAME="sweepo-server"
HOST_PORT="1969"
CONTAINER_PORT="1969"

echo "🚀 Starting Sweepo Server Docker Deployment..."
echo "================================================"

# Function to check if Docker is running
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        echo "❌ Error: Docker is not running. Please start Docker and try again."
        exit 1
    fi
    echo "✅ Docker is running"
}

# Function to stop and remove existing container
cleanup_existing_container() {
    echo "🧹 Checking for existing container..."
    
    if docker ps -q -f name=$CONTAINER_NAME | grep -q .; then
        echo "🛑 Stopping existing container: $CONTAINER_NAME"
        docker stop $CONTAINER_NAME
    fi
    
    if docker ps -aq -f name=$CONTAINER_NAME | grep -q .; then
        echo "🗑️  Removing existing container: $CONTAINER_NAME"
        docker rm $CONTAINER_NAME
    fi
    
    echo "✅ Container cleanup completed"
}

# Function to build Docker image
build_image() {
    echo "🔨 Building Docker image: $IMAGE_NAME"
    docker build -t $IMAGE_NAME .
    echo "✅ Docker image built successfully"
}

# Function to run the container
run_container() {
    echo "🚀 Starting new container: $CONTAINER_NAME"
    docker run -d \
        --name $CONTAINER_NAME \
        -p $HOST_PORT:$CONTAINER_PORT \
        -e SWEEPO_FROM_EMAIL_PASSWORD="${SWEEPO_FROM_EMAIL_PASSWORD}" \
        --restart unless-stopped \
        $IMAGE_NAME
    
    echo "✅ Container started successfully"
    echo "📡 Server is running on http://localhost:$HOST_PORT"
}

# Function to show container status
show_status() {
    echo ""
    echo "📊 Container Status:"
    echo "==================="
    docker ps -f name=$CONTAINER_NAME --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    
    echo ""
    echo "🔍 Container Logs (last 10 lines):"
    echo "=================================="
    docker logs --tail 10 $CONTAINER_NAME
}

# Function to cleanup unused files
cleanup_files() {
    echo "🧹 Cleaning up unused files..."
    
    if [ -f ".env.temp" ]; then
        echo "🗑️  Removing unused .env.temp file"
        rm .env.temp
        echo "✅ .env.temp removed"
    else
        echo "ℹ️  .env.temp file not found (already clean)"
    fi
}

# Main execution
main() {
    echo "Starting deployment process..."
    
    check_docker
    cleanup_existing_container
    cleanup_files
    build_image
    run_container
    show_status
    
    echo ""
    echo "🎉 Deployment completed successfully!"
    echo "🌐 Access your Sweepo server at: http://localhost:$HOST_PORT"
    echo "📋 API documentation: http://localhost:$HOST_PORT/swagger"
    echo "🔍 View logs: docker logs -f $CONTAINER_NAME"
    echo "🛑 Stop server: docker stop $CONTAINER_NAME"
}

# Handle script arguments
case "${1:-}" in
    "logs")
        echo "📋 Showing container logs..."
        docker logs -f $CONTAINER_NAME
        ;;
    "stop")
        echo "🛑 Stopping container..."
        docker stop $CONTAINER_NAME
        echo "✅ Container stopped"
        ;;
    "restart")
        echo "🔄 Restarting container..."
        docker restart $CONTAINER_NAME
        echo "✅ Container restarted"
        ;;
    "status")
        show_status
        ;;
    "clean")
        echo "🧹 Cleaning up everything..."
        cleanup_existing_container
        docker rmi $IMAGE_NAME 2>/dev/null || echo "ℹ️  Image not found"
        echo "✅ Cleanup completed"
        ;;
    *)
        main
        ;;
esac
