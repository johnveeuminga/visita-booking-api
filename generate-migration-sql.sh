#!/bin/bash

# EF Core Migration Script Generator
# Usage: ./generate-migration-sql.sh [from-migration] [to-migration]

set -e

# Configuration
MIGRATIONS_DIR="./Migrations"
OUTPUT_DIR="./sql-migrations"
PROJECT_NAME="visita-booking-api"

# Create output directory
mkdir -p "$OUTPUT_DIR"

echo "🗄️ EF Core Migration SQL Generator"
echo "================================="
echo ""

# Function to generate SQL script
generate_sql() {
    local from_migration="$1"
    local to_migration="$2"
    local output_file="$3"
    local description="$4"
    
    echo "📝 Generating SQL: $description"
    
    if [ -n "$from_migration" ] && [ -n "$to_migration" ]; then
        # Generate script from specific migration to specific migration
        dotnet ef migrations script "$from_migration" "$to_migration" --output "$output_file" --verbose
        echo "   Range: $from_migration → $to_migration"
    elif [ -n "$from_migration" ]; then
        # Generate script from specific migration to latest
        dotnet ef migrations script "$from_migration" --output "$output_file" --verbose
        echo "   From: $from_migration → Latest"
    else
        # Generate complete migration script
        dotnet ef migrations script --output "$output_file" --verbose
        echo "   Complete migration script"
    fi
    
    echo "   Output: $output_file"
    echo ""
}

# Check if EF tools are available
if ! dotnet ef --version &> /dev/null; then
    echo "❌ Error: dotnet-ef tool not found"
    echo "Install it with: dotnet tool install --global dotnet-ef"
    exit 1
fi

# Get current timestamp for file naming
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

# Parse arguments
FROM_MIGRATION="$1"
TO_MIGRATION="$2"

if [ -n "$FROM_MIGRATION" ] && [ -n "$TO_MIGRATION" ]; then
    # Generate specific range
    OUTPUT_FILE="$OUTPUT_DIR/migration_${FROM_MIGRATION}_to_${TO_MIGRATION}_${TIMESTAMP}.sql"
    generate_sql "$FROM_MIGRATION" "$TO_MIGRATION" "$OUTPUT_FILE" "Migration range: $FROM_MIGRATION to $TO_MIGRATION"
elif [ -n "$FROM_MIGRATION" ]; then
    # Generate from specific migration to latest
    OUTPUT_FILE="$OUTPUT_DIR/migration_from_${FROM_MIGRATION}_${TIMESTAMP}.sql"
    generate_sql "$FROM_MIGRATION" "" "$OUTPUT_FILE" "From $FROM_MIGRATION to latest"
else
    # Generate all standard variations
    echo "📋 Generating multiple migration scripts..."
    echo ""
    
    # Complete migration script (for fresh database)
    generate_sql "" "" "$OUTPUT_DIR/complete_migration_${TIMESTAMP}.sql" "Complete migration (fresh database)"
    
    # Recent migrations (last few migrations for incremental updates)
    RECENT_MIGRATION="20250914083847_AddAccommodationEntity"
    generate_sql "$RECENT_MIGRATION" "" "$OUTPUT_DIR/recent_migrations_${TIMESTAMP}.sql" "Recent migrations from $RECENT_MIGRATION"
    
    # Latest single migration
    LATEST_MIGRATION=$(ls -1 $MIGRATIONS_DIR/*.cs | grep -v Designer | grep -v ApplicationDbContextModelSnapshot | tail -1 | basename | cut -d'_' -f1)
    if [ -n "$LATEST_MIGRATION" ]; then
        PREV_MIGRATION=$(ls -1 $MIGRATIONS_DIR/*.cs | grep -v Designer | grep -v ApplicationDbContextModelSnapshot | tail -2 | head -1 | basename | cut -d'_' -f1)
        if [ -n "$PREV_MIGRATION" ]; then
            generate_sql "$PREV_MIGRATION" "$LATEST_MIGRATION" "$OUTPUT_DIR/latest_migration_only_${TIMESTAMP}.sql" "Latest migration only"
        fi
    fi
fi

echo "✅ SQL migration scripts generated successfully!"
echo ""
echo "📁 Output directory: $OUTPUT_DIR"
echo ""
echo "🔍 Generated files:"
ls -la "$OUTPUT_DIR"/*.sql 2>/dev/null | tail -5
echo ""

echo "📋 Next steps:"
echo "1. Review the generated SQL files"
echo "2. Test on staging database first"
echo "3. Backup production database"
echo "4. Execute the appropriate SQL file on production"
echo ""

echo "💡 Usage examples:"
echo "  ./generate-migration-sql.sh                                    # Generate all standard scripts"
echo "  ./generate-migration-sql.sh 20250912121018_InitialMigrations   # From specific migration to latest"
echo "  ./generate-migration-sql.sh InitialMigrations AddAccommodation # Specific range"