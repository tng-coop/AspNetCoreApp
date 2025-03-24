#!/usr/bin/env bash

set -e

# Go into the Client directory
cd Client

# Clearly list all relevant files
files=(
    "BlazorWasm.csproj"
    "Program.cs"
    "App.razor"
    "_Imports.razor"
    "Shared/MainLayout.razor"
    "Pages/HelloWorld.razor"
    "wwwroot/index.html"
    "wwwroot/css/app.css"
)

# Display each file's content explicitly
for file in "${files[@]}"; do
    echo "---- Start of: Client/$file ----"
    cat "$file"
    echo "---- End of: Client/$file ----"
    echo

done
