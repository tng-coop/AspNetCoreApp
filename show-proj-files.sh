#!/bin/bash

# Change to the script's own directory reliably
cd "$(dirname "$(realpath "$0")")"

# Files explicitly required
required_files=(
    "Program.cs"
    "*.csproj"
    "./Pages/*.cshtml"
    "./Pages/*.cshtml.cs"
    "./Models/*.cs"
    "./Data/*.cs"
    "./wwwroot/index.html"
    "./Areas/Identity/Pages/Account/*.cshtml"
    "./Areas/Identity/Pages/Account/*.cshtml.cs"
    "./Areas/Identity/Pages/Account/Manage/Enable*.cshtml"
    "./Areas/Identity/Pages/Account/Manage/Enable*.cshtml.cs"
    "./*.sh"
    "./*.env"
    "./appsettings.json"
    "./appsettings.*.json"
    "./Dockerfile.*"
    "PlaywrightTests/*.ts"
)

# Display contents of required files
echo "=== Required Files Content ==="
for file in "${required_files[@]}"; do
    for f in $file; do
        if [ -f "$f" ]; then
            echo "---- Start of: $f ----"
            cat "$f"
            echo "---- End of: $f ----"
            echo
        fi
    done
done

# Explicitly excluding unnecessary directories
exclude_dirs=(
    "./bin"
    "./obj"
    "./.git"
    "./PlaywrightTests/node_modules"
    "./.github"
    "./Areas/Identity"
)

# Verifying existence of key JS resources
echo "=== Resource File checks  ==="
js_files=(
    "./wwwroot/js/qr.js"
    "./wwwroot/lib/qrcode.js"
)

for jsfile in "${js_files[@]}"; do
    [ -f "$jsfile" ] && echo "$jsfile: Exists" || echo "$jsfile: Missing"
done



# End of script