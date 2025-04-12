#!/bin/bash

# Files to display
FILES=(
    "./Services/LocalizationService.cs"
    "./Components/Layout/LangSwitchButton.razor"
    "./Program.cs"
    "./Components/App.razor"
    "./Data/ApplicationUser.cs"
)

# Print filenames and contents clearly
for file in "${FILES[@]}"; do
    echo "======== ${file} ========"

    if [ -f "${file}" ]; then
        cat "${file}"
    else
        echo "⚠️ File not found: ${file}"
    fi

    echo ""
done
