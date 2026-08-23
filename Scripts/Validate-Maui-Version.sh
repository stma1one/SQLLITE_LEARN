#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}"

WHAT_NEW="https://learn.microsoft.com/en-us/dotnet/maui/whats-new/?view=net-maui-10.0"
DOTNET10="https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0"
DOTNET11="https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-11?view=net-maui-11.0"

echo -e "\033[0;36mChecking Microsoft Learn MAUI documentation...\033[0m"

REACHABLE=1
for url in "$WHAT_NEW" "$DOTNET10" "$DOTNET11"; do
    if command -v curl >/dev/null 2>&1; then
        if ! curl -s -f --max-time 15 -o /dev/null "$url"; then
            REACHABLE=0
            echo -e "\033[0;33mWARNING: Could not retrieve $url\033[0m"
        fi
    elif command -v wget >/dev/null 2>&1; then
        if ! wget -q --spider --timeout=15 "$url"; then
            REACHABLE=0
            echo -e "\033[0;33mWARNING: Could not retrieve $url\033[0m"
        fi
    fi
done

CSPROJ=$(find "$PROJECT_ROOT" -type f -name "*.csproj" -not -path "*/bin/*" -not -path "*/obj/*" | head -n 1)

if [ -z "$CSPROJ" ]; then
    echo -e "\033[0;31mNo .csproj found.\033[0m"
    exit 2
fi

TFM=$(grep -o -E '<TargetFrameworks?>[^<]+' "$CSPROJ" | sed -E 's/<TargetFrameworks?>[[:space:]]*//' | head -n 1 || true)

echo "Project: $CSPROJ"
echo "Target framework: $TFM"

if [[ "$TFM" =~ net11 ]]; then
    echo -e "\033[0;33mNOTICE: .NET 11 MAUI is currently preview. Do not use preview-only APIs unless explicitly required.\033[0m"
elif [[ "$TFM" =~ net10 ]]; then
    echo -e "\033[0;32mTarget appears to be .NET 10.\033[0m"
else
    echo -e "\033[0;33mTarget is not clearly net10/net11. Verify the instructor's target version against Microsoft Learn.\033[0m"
fi

if [ $REACHABLE -eq 0 ]; then
    echo -e "\033[0;33mRESULT: CHECK REQUIRED - live documentation could not be fully retrieved.\033[0m"
    exit 1
fi

echo -e "\033[0;32mRESULT: PASS\033[0m"
exit 0
