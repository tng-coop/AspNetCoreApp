#!/bin/bash

# Script to recursively list all files (excluding common noise directories),
# and print each filename followed by its contents clearly.

find . \
  -type d \( -name '.git' -o -name 'bin' -o -name 'obj' -o -name '.vscode' -o -name '.idea' \) -prune -false \
  -o -type f -print | while read -r file; do
    echo -e "\n===== $file =====\n"
    cat "$file"
done

