#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}"

PROGRAM=$(find "$PROJECT_ROOT" -type f -name "MauiProgram.cs" -not -path "*/bin/*" -not -path "*/obj/*" | head -n 1)

if [ -z "$PROGRAM" ]; then
    echo -e "\033[0;31mMauiProgram.cs not found.\033[0m"
    exit 2
fi

PROGRAM_TEXT=$(cat "$PROGRAM")

find "$PROJECT_ROOT" -type f -name "*.cs" -not -path "*/bin/*" -not -path "*/obj/*" -not -path "*/Tests/*" | while read -r file; do
    if [[ "$file" =~ /Services/ || "$file" =~ /ViewModels/ || "$file" =~ \\Services\\ || "$file" =~ \\ViewModels\\ ]]; then
        name=$(basename "$file" .cs)
        if [[ "$name" == *"BaseViewModel"* ]]; then
            continue
        fi

        if echo "$PROGRAM_TEXT" | grep -F -q "$name"; then
            echo -e "\033[0;32m[OK] $name appears in MauiProgram.cs\033[0m"
        else
            echo -e "\033[0;33m[CHECK] $name does not appear in MauiProgram.cs (may be intentional).\033[0m"
        fi
    fi
done

echo -e "\033[0;90mConservative audit: verify actual DI resolution paths.\033[0m"
exit 0
