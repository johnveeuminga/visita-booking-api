#!/bin/bash

# Visita Booking API - Local Development Setup Script

echo "🚀 Setting up Visita Booking API for local development..."

# Create necessary directories
echo "📁 Creating required directories..."
mkdir -p docker/redis
mkdir -p docker/mysql/data
mkdir -p logs

# Start services
echo "🐳 Starting Docker containers..."
docker-compose up -d mysql redis

# Wait for services to be ready
echo "⏳ Waiting for database to be ready..."
until docker-compose exec mysql mysqladmin ping -h"localhost" --silent; do
    echo "Waiting for database connection..."
    sleep 2
done

echo "⏳ Waiting for Redis to be ready..."
until docker-compose exec redis redis-cli ping; do
    echo "Waiting for Redis connection..."
    sleep 1
done

# Check if we need to run migrations
echo "🔍 Checking database status..."
if docker-compose exec mysql mysql -uvisitauser -pVisitaUser123! -e "USE VisitaBookingDB; SHOW TABLES;" > /dev/null 2>&1; then
    echo "✅ Database tables found"
else
    echo "📦 Database tables not found. You may need to run migrations:"
    echo "   dotnet ef database update"
fi

echo ""
echo "🎉 Local development environment is ready!"
echo ""
echo "📊 Service URLs:"
echo "   API: http://localhost:8080"
echo "   MySQL: localhost:3306"
echo "   Redis: localhost:6379"
echo "   Redis GUI: http://localhost:8081 (run with --profile tools)"
echo ""
echo "🔧 Useful commands:"
echo "   Start all services: docker-compose up -d"
echo "   Start with Redis GUI: docker-compose --profile tools up -d"
echo "   View logs: docker-compose logs -f [service]"
echo "   Stop all services: docker-compose down"
echo "   Reset all data: docker-compose down -v"
echo ""
echo "🛠️  Redis CLI access:"
echo "   docker-compose exec redis redis-cli -a VisitaRedis123!"
echo ""