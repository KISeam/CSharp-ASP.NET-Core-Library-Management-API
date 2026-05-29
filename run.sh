#!/bin/bash

# 🚀 LibraryAPI - Quick Start Script
# This script starts the API server with localStorage token persistence

echo "╔════════════════════════════════════════════════════════════╗"
echo "║     📚 LibraryAPI - Starting with Auth System Ready      ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

PROJECT_PATH="/media/md-seam/New_Volume4/C# Projects/Web API Project/Library Management API"

if [ ! -d "$PROJECT_PATH" ]; then
    echo "❌ Project path not found: $PROJECT_PATH"
    exit 1
fi

cd "$PROJECT_PATH"

echo "📋 Project: LibraryAPI"
echo "📁 Location: $PROJECT_PATH"
echo ""

echo "🔧 Building project..."
dotnet build --configuration Debug --no-restore > /dev/null 2>&1

if [ $? -eq 0 ]; then
    echo "✅ Build successful"
else
    echo "❌ Build failed"
    exit 1
fi

echo ""
echo "🚀 Starting server..."
echo ""
echo "═════════════════════════════════════════════════════════════"
echo "✅ Features Enabled:"
echo "   📦 LocalStorage Token Persistence"
echo "   🌙 Swagger UI Dark Mode"
echo "   🔐 JWT Authentication"
echo "   🔄 Auto Token Refresh"
echo "═════════════════════════════════════════════════════════════"
echo ""
echo "🌐 Access Points:"
echo "   🔑 Auth Test Page:  http://localhost:5000"
echo "   📚 Swagger UI:      http://localhost:5000/swagger"
echo "   📖 Docs:            See LOCALSTORAGE_AUTH.md"
echo ""
echo "👤 Default Credentials:"
echo "   Email:    admin@library.com"
echo "   Password: Admin@123"
echo ""
echo "═════════════════════════════════════════════════════════════"
echo ""

# Start the application
dotnet run
