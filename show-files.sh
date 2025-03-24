#!/bin/bash

name_only=false
directory="."

# Parse arguments
while [[ "$#" -gt 0 ]]; do
  case "$1" in
    -n|--name-only)
      name_only=true
      shift
      ;;
    -*)
      echo "Unknown option: $1"
      exit 1
      ;;
    *)
      directory="$1"
      shift
      ;;
  esac
done

cd "$directory" || { echo "Failed to change directory to $directory"; exit 1; }

FILES=$(find . -type f | egrep -vi '(./git|./published|node_module|logs|docs|./bin|./out|/obj|/.git|PlaywrightTests|asset|migratio|\.sh$|launchSett|\.txt$|\.md$|\.css$|\.mjs$|\.env$|localhost|Dockerfile|package-lock|package.json|\.ps1$)')

for f in $FILES
do
  if [ "$name_only" = true ]; then
    echo "$f"
  else
    echo "$f"
    echo "----------------"
    cat "$f"
    echo "----------------"
  fi
done