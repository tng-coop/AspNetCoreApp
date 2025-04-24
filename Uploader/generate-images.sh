#!/usr/bin/env bash
set -euo pipefail

# where to put the generated images
IMG_DIR="images"

# make sure the folder exists
mkdir -p "$IMG_DIR"

# generate 10 500×500 plasma fractal images
for i in $(seq 1 10); do
  convert -size 500x500 plasma:fractal "$IMG_DIR/colorful_${i}.png"
  echo "→ $IMG_DIR/colorful_${i}.png"
done

echo "Done! Generated 10 images in '$IMG_DIR'."
