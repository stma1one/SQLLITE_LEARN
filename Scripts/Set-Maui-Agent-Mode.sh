#!/usr/bin/env bash
set -euo pipefail

MODE="${1:-true}"
PROJECT_ROOT="${2:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}"
CONFIG_FILE="$PROJECT_ROOT/.agents/MAUI-Agent-Mode.json"

if [ "$MODE" != "true" ] && [ "$MODE" != "false" ]; then
    echo "Usage: ./Set-Maui-Agent-Mode.sh [true|false]"
    exit 1
fi

mkdir -p "$(dirname "$CONFIG_FILE")"
cat << EOF > "$CONFIG_FILE"
{
  "studentCapabilityMode": $MODE,
  "description": {
    "true": "MAXIMIZE STUDENT CAPABILITY: staged planning, concept gates, ownership negotiation, student practice, and verification.",
    "false": "MAXIMIZE CODE GENERATED: implement complete feature slices efficiently while preserving verification and learning documentation when configured."
  }
}
EOF

echo "Set studentCapabilityMode to $MODE in $CONFIG_FILE"
