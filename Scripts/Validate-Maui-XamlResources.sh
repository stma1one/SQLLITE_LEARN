#!/usr/bin/env bash
set -u

PROJECT_ROOT="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}"

if [ ! -d "$PROJECT_ROOT" ]; then
    echo -e "\033[0;31mProject root not found: $PROJECT_ROOT\033[0m"
    exit 2
fi

XAML_FILES=$(find "$PROJECT_ROOT" -type f -name "*.xaml" -not -path "*/bin/*" -not -path "*/obj/*")
FILE_COUNT=$(echo "$XAML_FILES" | grep -c "\.xaml" || true)

if [ "$FILE_COUNT" -eq 0 ]; then
    echo -e "\033[0;31mNo XAML files found.\033[0m"
    exit 2
fi

TMP_DEFINED=$(mktemp)
TMP_REFERENCES=$(mktemp)
TMP_DUPLICATES=$(mktemp)
TMP_MISSING=$(mktemp)
trap 'rm -f "$TMP_DEFINED" "$TMP_REFERENCES" "$TMP_DUPLICATES" "$TMP_MISSING"' EXIT

while IFS= read -r file; do
    if [ -f "$file" ]; then
        # Extract x:Key="key_name"
        keys=$(grep -o -E 'x:Key="[^"]+"' "$file" 2>/dev/null | sed -E 's/x:Key="([^"]+)"/\1/' || true)
        if [ -n "$keys" ]; then
            while IFS= read -r key; do
                if [ -n "$key" ]; then
                    echo "$key|$file" >> "$TMP_DEFINED"
                fi
            done <<< "$keys"
        fi

        # Extract StaticResource and DynamicResource references
        refs=$(grep -o -E '\{(StaticResource|DynamicResource)[[:space:]]+[^\}[:space:]]+' "$file" 2>/dev/null | \
            sed -E 's/\{(StaticResource|DynamicResource)[[:space:]]+//' || true)
        if [ -n "$refs" ]; then
            while IFS= read -r ref_key; do
                if [ -n "$ref_key" ]; then
                    echo "$ref_key|$file" >> "$TMP_REFERENCES"
                fi
            done <<< "$refs"
        fi
    fi
done <<< "$XAML_FILES"

DEFINED_KEYS=$(cut -d'|' -f1 "$TMP_DEFINED" 2>/dev/null || true)
if [ -s "$TMP_DEFINED" ]; then
    DEFINED_COUNT=$(echo "$DEFINED_KEYS" | grep -c -v "^$" || true)
else
    DEFINED_COUNT=0
fi
REF_COUNT=$(wc -l < "$TMP_REFERENCES" 2>/dev/null || echo 0)

# Check for duplicate defined keys
if [ -s "$TMP_DEFINED" ]; then
    sort "$TMP_DEFINED" | awk -F'|' '
    {
        key = $1; file = $2;
        if (key in seen) {
            print key "|" seen[key] "|" file;
        } else {
            seen[key] = file;
        }
    }' > "$TMP_DUPLICATES"
fi

# Check for missing references
if [ -s "$TMP_REFERENCES" ]; then
    while IFS='|' read -r ref_key ref_file; do
        if [ -n "$ref_key" ]; then
            if ! grep -q "^$ref_key|" "$TMP_DEFINED" 2>/dev/null; then
                echo "$ref_key|$ref_file" >> "$TMP_MISSING"
            fi
        fi
    done < "$TMP_REFERENCES"
fi

MISSING_COUNT=$(wc -l < "$TMP_MISSING" 2>/dev/null || echo 0)
DUPLICATE_COUNT=$(wc -l < "$TMP_DUPLICATES" 2>/dev/null || echo 0)

echo -e "\033[0;36m==============================================\033[0m"
echo -e "\033[0;36m .NET MAUI XAML STYLE/RESOURCE AUDIT\033[0m"
echo -e "\033[0;36m==============================================\033[0m"
echo "XAML files scanned: $FILE_COUNT"
echo "Resource keys found: $DEFINED_COUNT"
echo "References found: $REF_COUNT"
echo ""

if [ "$MISSING_COUNT" -gt 0 ]; then
    echo -e "\033[0;31mMISSING RESOURCE REFERENCES\033[0m"
    sort -u "$TMP_MISSING" | while IFS='|' read -r m_key m_file; do
        echo -e "\033[0;31m[$m_key]\033[0m"
        echo "    $m_file"
    done
    echo ""
fi

if [ "$DUPLICATE_COUNT" -gt 0 ]; then
    echo -e "\033[0;33mPOSSIBLE DUPLICATE RESOURCE KEYS\033[0m"
    sort -u "$TMP_DUPLICATES" | while IFS='|' read -r d_key d_first d_dup; do
        echo -e "\033[0;33m[$d_key]\033[0m"
        echo "    first:     $d_first"
        echo "    duplicate: $d_dup"
    done
    echo ""
fi

if [ "$MISSING_COUNT" -eq 0 ] && [ "$DUPLICATE_COUNT" -eq 0 ]; then
    echo -e "\033[0;32mRESULT: PASS\033[0m"
    exit 0
fi

echo -e "\033[0;33mRESULT: CHECK REQUIRED\033[0m"
exit 1
