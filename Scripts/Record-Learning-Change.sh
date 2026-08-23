#!/usr/bin/env bash
set -euo pipefail

TITLE="${1:-}"
WHAT_CHANGED="${2:-}"
WHY="${3:-}"
CONCEPT="${4:-}"
VERIFICATION="${5:-dotnet build; dotnet test}"
MICROSOFT_LEARN="${6:-}"

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
LEARNING_DIR="$ROOT/Learning"
BOOK="$LEARNING_DIR/MAUI-Learning-Book.md"

mkdir -p "$LEARNING_DIR"

if [ ! -f "$BOOK" ]; then
    cat << 'EOF' > "$BOOK"
# MAUI Learning Book

This is the student's living e-learning book.

EOF
fi

LESSON_COUNT=$(grep -c "^# Lesson " "$BOOK" 2>/dev/null || true)
NUMBER=$((LESSON_COUNT + 1))
DATE=$(date "+%Y-%m-%d %H:%M")

cat << EOF >> "$BOOK"

# Lesson $NUMBER — $TITLE

_Date: $DATE_

## What changed?
$WHAT_CHANGED

## Why did we change it?
$WHY

## What MAUI/C# concept did we learn?
$CONCEPT

## How to verify

\`\`\`text
$VERIFICATION
\`\`\`

## Microsoft Learn
$MICROSOFT_LEARN

## Check yourself

1. Can you explain the change without reading the code?
2. Can you explain where the state and logic belong?

EOF

echo -e "\033[0;32mLearning lesson $NUMBER added.\033[0m"
