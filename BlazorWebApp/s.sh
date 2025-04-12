#!/bin/bash

# Files to display
FILES=(
    # "./Services/LocalizationService.cs"
    # "./Components/Layout/LangSwitchButton.razor"
    # "./Program.cs"
    # "./Components/App.razor"
    # "./Data/ApplicationUser.cs"
    
    ./Components/Layout
    ./Components/Layout/LangSwitchButton.razor
    ./Components/Layout/NavMenu.razor
    ./Components/Layout/MainLayout.razor
    ./Components/Layout/NavMenu.razor.css
    ./Components/Layout/MainLayout.razor.css
    ./Components/Account/Shared/ManageLayout.razor
    ./Components/Pages/Weather.razor
    ./Components/Pages/Upload.razor
    ./Components/Pages/Home.razor
    ./Components/Pages/Error.razor
    ./Components/Pages/Auth.razor
    ./Components/Pages/Counter.razor
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
