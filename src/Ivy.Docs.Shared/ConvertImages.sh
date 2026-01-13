#!/bin/bash

# WARNING: This script replaces all pngs in Assets with webps.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ASSETS_DIR="$SCRIPT_DIR/Assets"

shopt -s nullglob          # makes *.png expand to nothing if there are no matches
for f in "$ASSETS_DIR"/*.png; do
    webp="${f%.png}.webp"
    magick "$f" -strip -quality 80 "$webp" && rm "$f"
done
