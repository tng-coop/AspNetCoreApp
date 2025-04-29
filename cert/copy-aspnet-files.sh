#!/usr/bin/env bash
set -euo pipefail

# Source cert folder on yasu’s account
SRC_DIR="/home/yasu/co/AspNetCoreApp/cert"

# Destination is wherever you run this script
DEST_DIR="$(pwd)"

# Only files beginning with "aspnet" in the root of SRC_DIR
PATTERN="aspnet*"

# Sanity check
if [ ! -d "$SRC_DIR" ]; then
  echo "❌ Source directory '$SRC_DIR' not found!" >&2
  exit 1
fi

# Gather matching files
shopt -s nullglob
files=( "$SRC_DIR"/$PATTERN )

if [ ${#files[@]} -eq 0 ]; then
  echo "⚠️  No files matching '$PATTERN' in '$SRC_DIR'." >&2
  exit 0
fi

echo "🔄 Copying ${#files[@]} file(s) from '$SRC_DIR' → '$DEST_DIR'…"
for src in "${files[@]}"; do
  sudo cp -p "$src" "$DEST_DIR"/
done

# Fix ownership so the invoking user owns them
USER="$(whoami)"
GROUP="$(id -gn)"
echo "🔧 Chowning to $USER:$GROUP…"
sudo chown "$USER":"$GROUP" "$DEST_DIR"/$PATTERN

echo "✅ Done."
