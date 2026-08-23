#!/usr/bin/env bash
set -euo pipefail

ROOT="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}"

SKILLS=("maui-k12-baton-workflow")

for skill in "${SKILLS[@]}"; do
    mkdir -p "$ROOT/.agents/skills/$skill"
done

mkdir -p "$ROOT/Learning"
mkdir -p "$ROOT/.agents"

CONFIG_PATH="$ROOT/.agents/MAUI-Agent-Mode.json"
if [ ! -f "$CONFIG_PATH" ]; then
    cat << 'EOF' > "$CONFIG_PATH"
{
  "studentCapabilityMode": true,
  "description": {
    "true": "MAXIMIZE STUDENT CAPABILITY",
    "false": "MAXIMIZE CODE GENERATED"
  }
}
EOF
fi

CURRENT_PHASE="$ROOT/Learning/Current-Phase.md"
if [ ! -f "$CURRENT_PHASE" ]; then
    cat << 'EOF' > "$CURRENT_PHASE"
# Current Phase

Status: NOT_STARTED

Mode is controlled by `.agents/MAUI-Agent-Mode.json`.

The orchestrator creates phase details when the first feature begins.
EOF
fi

MASTERY="$ROOT/Learning/Student-Mastery.md"
if [ ! -f "$MASTERY" ]; then
    cat << 'EOF' > "$MASTERY"
# Student Mastery

Use:
- ⚪ Not introduced
- 🟡 Practicing
- 🟢 Understands
- 🔵 Can work independently
EOF
fi

LEARNING_BOOK="$ROOT/Learning/MAUI-Learning-Book.md"
if [ ! -f "$LEARNING_BOOK" ]; then
    cat << 'EOF' > "$LEARNING_BOOK"
# MAUI Learning Book

This is the student's append-only learning book.
EOF
fi

echo -e "\033[0;32mMAUI K12 skill structure and learning state are ready.\033[0m"
echo -e "\033[0;36mDefault mode: MAXIMIZE STUDENT CAPABILITY\033[0m"
