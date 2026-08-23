#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}"
FOUND=0

echo "Checking for obsolete MAUI APIs..."

check_rule() {
    local pattern="$1"
    local name="$2"
    local replacement="$3"
    local source="$4"

    while IFS= read -r file; do
        if [ -f "$file" ]; then
            while IFS=: read -r line_num line_content; do
                if [ -n "$line_num" ]; then
                    FOUND=1
                    echo -e "\033[0;33m[REVIEW] $name -> $file:$line_num\033[0m"
                    echo "  Replacement/review: $replacement"
                    echo "  Microsoft Learn: $source"
                fi
            done < <(grep -n -E "$pattern" "$file" 2>/dev/null || true)
        fi
    done < <(find "$PROJECT_ROOT" -type f \( -name "*.cs" -o -name "*.xaml" \) -not -path "*/bin/*" -not -path "*/obj/*")
}

check_rule '\bPage\.IsBusy\b' 'Page.IsBusy' 'ViewModel IsLoading + ActivityIndicator/other current loading UX' 'https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0'
check_rule '\bListView\b' 'ListView' 'Review current CollectionView guidance and project requirements' 'https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0'
check_rule '\bMessagingCenter\b' 'MessagingCenter' 'Review the current MAUI messaging guidance for the target version' 'https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0'

echo ""
echo -e "\033[0;90mThis script is a guardrail. Compiler warnings, analyzers, and current Microsoft Learn pages remain authoritative.\033[0m"

if [ $FOUND -eq 1 ]; then
    echo -e "\033[0;33mRESULT: CHECK REQUIRED\033[0m"
    exit 1
fi

echo -e "\033[0;32mRESULT: PASS\033[0m"
exit 0
