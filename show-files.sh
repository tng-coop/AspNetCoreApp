#!/bin/bash

name_only=false
directory="."
regex_filter=""

# Function to display help message
display_help() {
  echo "Usage: $0 [OPTIONS] [DIRECTORY]"
  echo
  echo "Options:"
  echo "  -n, --name-only     Display only the file names, not their content."
  echo "  -r, --regex PATTERN  Filter files using the provided regex pattern."
  echo "  -h, --help          Display this help message."
  echo
  echo "Arguments:"
  echo "  DIRECTORY           Directory to search in. Defaults to the current directory."
  echo
  echo "Example usage:"
  echo "  $0 -r '.*\.cs'          Search for .cs files, excluding certain directories."
  echo "  $0 -n -r '.*\.cs'       Display only file names for .cs files."
  echo "  $0 -h                  Display this help message."
  exit 0
}

# Parse arguments
while [[ "$#" -gt 0 ]]; do
  case "$1" in
    -n|--name-only)
      name_only=true
      shift
      ;;
    -r|--regex)
      regex_filter="$2"
      shift 2
      ;;
    -h|--help)
      display_help
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

# Base egrep pattern for excluding unwanted files and directories
exclude_pattern='(./git|./published|node_module|logs|docs|./bin|./out|/obj|/.git|PlaywrightTests|asset|migratio|\.sh$|launchSett|\.txt$|\.md$|\.css$|\.mjs$|\.env$|localhost|Dockerfile|package-lock|package.json|\.ps1$)'

# Combine the exclude pattern with the user-provided regex filter if any
if [[ -n "$regex_filter" ]]; then
  FILES=$(find . -type f | egrep -vi "$exclude_pattern" | egrep -i "$regex_filter")
else
  FILES=$(find . -type f | egrep -vi "$exclude_pattern")
fi

# Process files
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
