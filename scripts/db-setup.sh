#!/bin/bash

# Database Migration and Setup Script for Trauma-Informed Platform
# This script initializes the PostgreSQL database and applies all EF Core migrations

set -e

echo "🏗️  Trauma-Informed Platform - Database Setup"
echo "================================================"

# Check if Docker is running
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed or not in PATH"
    exit 1
fi

echo "✅ Docker is available"

# Start Docker containers
echo ""
echo "🐳 Starting Docker services..."
npm run docker:up
echo "✅ Docker services started"

# Wait for PostgreSQL to be ready
echo ""
echo "⏳ Waiting for PostgreSQL to be ready..."
sleep 10

# Navigate to Identity API
echo ""
echo "📁 Navigating to Identity API..."
cd services/identity-api

# Apply migrations
echo ""
echo "🗄️  Applying Entity Framework Core migrations..."
if command -v dotnet &> /dev/null; then
    dotnet ef database update
    echo "✅ Migrations applied successfully"
else
    echo "❌ dotnet CLI is not installed or not in PATH"
    exit 1
fi

# Return to root
cd ../..

echo ""
echo "================================================"
echo "✅ Database setup complete!"
echo ""
echo "Database Details:"
echo "  Host:     localhost"
echo "  Port:     5432"
echo "  Database: trauma_platform_identity"
echo "  User:     postgres"
echo "  Password: postgres"
echo ""
echo "To view logs:"
echo "  npm run docker:logs"
echo ""
echo "To stop services:"
echo "  npm run docker:down"
echo "================================================"
