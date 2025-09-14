#!/bin/bash

# Visita Booking API - Redis Cache Management Script

REDIS_PASSWORD="VisitaRedis123!"
REDIS_HOST="localhost"
REDIS_PORT="6379"

# Function to execute Redis commands
redis_cmd() {
    docker-compose exec redis redis-cli -a "$REDIS_PASSWORD" "$@"
}

# Function to show help
show_help() {
    echo "🔧 Redis Cache Management for Visita Booking API"
    echo ""
    echo "Usage: $0 [command] [options]"
    echo ""
    echo "Commands:"
    echo "  info           - Show Redis server info"
    echo "  stats          - Show cache statistics"
    echo "  keys [pattern] - List keys (default: visita:*)"
    echo "  get <key>      - Get value for a key"
    echo "  del <key>      - Delete a key"
    echo "  flush          - Flush all cache (BE CAREFUL!)"
    echo "  monitor        - Monitor Redis commands in real-time"
    echo "  warmup         - Trigger cache warmup (if implemented)"
    echo ""
    echo "Examples:"
    echo "  $0 stats"
    echo "  $0 keys 'room:*'"
    echo "  $0 get room:details:101"
    echo "  $0 del 'search:*'"
    echo ""
}

case "$1" in
    "info")
        echo "📊 Redis Server Information:"
        redis_cmd INFO server
        echo ""
        echo "💾 Memory Usage:"
        redis_cmd INFO memory | grep used_memory_human
        echo ""
        echo "📈 Stats:"
        redis_cmd INFO stats | grep -E "(total_commands_processed|total_connections_received|keyspace_hits|keyspace_misses)"
        ;;
    
    "stats")
        echo "📈 Cache Statistics:"
        redis_cmd INFO stats
        echo ""
        echo "🔑 Keyspace Info:"
        redis_cmd INFO keyspace
        ;;
    
    "keys")
        pattern=${2:-"visita:*"}
        echo "🔍 Keys matching pattern: $pattern"
        redis_cmd KEYS "$pattern"
        ;;
    
    "get")
        if [ -z "$2" ]; then
            echo "❌ Please provide a key to get"
            echo "Usage: $0 get <key>"
            exit 1
        fi
        echo "📄 Value for key: $2"
        redis_cmd GET "$2"
        ;;
    
    "del")
        if [ -z "$2" ]; then
            echo "❌ Please provide a key pattern to delete"
            echo "Usage: $0 del <key_or_pattern>"
            exit 1
        fi
        
        if [[ "$2" == *"*"* ]]; then
            echo "🗑️  Deleting keys matching pattern: $2"
            redis_cmd EVAL "return redis.call('del', unpack(redis.call('keys', ARGV[1])))" 0 "$2"
        else
            echo "🗑️  Deleting key: $2"
            redis_cmd DEL "$2"
        fi
        ;;
    
    "flush")
        echo "⚠️  WARNING: This will delete ALL cached data!"
        read -p "Are you sure? (y/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            redis_cmd FLUSHDB
            echo "✅ Cache flushed successfully"
        else
            echo "❌ Operation cancelled"
        fi
        ;;
    
    "monitor")
        echo "👁️  Monitoring Redis commands (Ctrl+C to stop):"
        redis_cmd MONITOR
        ;;
    
    "warmup")
        echo "🔥 Triggering cache warmup..."
        echo "This would typically call your API's cache warmup endpoint:"
        echo "curl -X POST http://localhost:8080/api/admin/cache/warmup"
        # Uncomment when endpoint is implemented:
        # curl -X POST http://localhost:8080/api/admin/cache/warmup
        ;;
    
    *)
        show_help
        ;;
esac