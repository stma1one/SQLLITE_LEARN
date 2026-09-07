#!/usr/bin/env bash
set -euo pipefail

TARGET="${1:-Project}"
PROJECT_PATH="${2:-.}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PACKAGE_ROOT="$(dirname "$SCRIPT_DIR")"
SKILL_SOURCE="$PACKAGE_ROOT/skills/maui-step-guide-author"

if [ ! -d "$SKILL_SOURCE" ]; then
    echo "Error: Cannot find skill source: $SKILL_SOURCE" >&2
    exit 1
fi

case "$TARGET" in
    Project)
        RESOLVED_PROJECT="$(cd "$PROJECT_PATH" && pwd)"

        PROJECT_AGENTS="$RESOLVED_PROJECT/.agents"
        PROJECT_SKILL_DEST="$PROJECT_AGENTS/skills/maui-step-guide-author"
        PROJECT_LEARNING_DEST="$RESOLVED_PROJECT/Learning"

        mkdir -p "$PROJECT_SKILL_DEST"
        cp -R "$SKILL_SOURCE/"* "$PROJECT_SKILL_DEST/"

        mkdir -p "$PROJECT_LEARNING_DEST/Guides"
        mkdir -p "$PROJECT_LEARNING_DEST/Mockups"
        mkdir -p "$PROJECT_LEARNING_DEST/Images"

        echo "Installed MAUI Step Guide Author skill into project: $RESOLVED_PROJECT"
        ;;
    Codex)
        DEST="$HOME/.codex/skills/maui-step-guide-author"
        mkdir -p "$DEST"
        cp -R "$SKILL_SOURCE/"* "$DEST/"
        echo "Installed Codex skill: $DEST"
        ;;
    ClaudeCode)
        DEST="$HOME/.claude/skills/maui-step-guide-author"
        mkdir -p "$DEST"
        cp -R "$SKILL_SOURCE/"* "$DEST/"
        echo "Installed Claude Code skill: $DEST"
        ;;
    *)
        echo "Usage: ./scripts/Install-Skill.sh [Project|Codex|ClaudeCode] [ProjectPath]"
        exit 1
        ;;
esac
