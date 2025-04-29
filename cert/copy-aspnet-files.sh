#!/usr/bin/env bash
set -euo pipefail

SRC_DIR="/home/yasu/co/AspNetCoreApp/cert"
DEST_DIR="$(pwd)"
PATTERN="aspnet*"

# Check via sudo if the source directory exists
if ! sudo test -d "$SRC_DIR"; then
  echo "❌ Source directory '$SRC_DIR' not found or inaccessible!" >&2
  exit 1
fi

# Get the list of matching files via sudo+find
mapfile -t files < <(sudo find "$SRC_DIR" -maxdepth 1 -type f -name "$PATTERN")

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
