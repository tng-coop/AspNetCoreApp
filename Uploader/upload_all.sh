#!/usr/bin/env bash
set -euo pipefail

# --- CONFIGURATION ----------------------------------------------------------
# Change this if you target a different framework or configuration:
CONFIGURATION="Release"
FRAMEWORK="net9.0"
PROJECT_DLL="bin/$CONFIGURATION/$FRAMEWORK/Uploader.dll"

IMAGEDIR="images"
# ---------------------------------------------------------------------------

# Build once
echo "Building Uploader project..."
dotnet build -c $CONFIGURATION

if [[ ! -f $PROJECT_DLL ]]; then
  echo "ERROR: could not find $PROJECT_DLL" >&2
  exit 1
fi

# Loop through all PNGs in the images directory
shopt -s nullglob
files=("$IMAGEDIR"/*.png)

if (( ${#files[@]} == 0 )); then
  echo "No PNG files found in '$IMAGEDIR'." >&2
  exit 1
fi

for img in "${files[@]}"; do
  echo
  echo "👉  Uploading: $img"
  dotnet "$PROJECT_DLL" "$img"
done
