#!/bin/bash

name_only=false
directory="."
include_regex=""
exclude_regex=""
extended=false

# Function to display help message
display_help() {
  echo "Usage: $0 [OPTIONS] [DIRECTORY]"
  echo
  echo "Options:"
  echo "  -n, --name-only         Display only the file names, not their content."
  echo "  -r, --regex PATTERN     Include files matching the provided regex pattern."
  echo "  -v, --invert PATTERN    Exclude files matching the provided regex pattern."
  echo "  -e, --extended          Include additional identity-related files."
  echo "  -a, --all               Include all files without default exclusions."
  echo "  -h, --help              Display this help message."
  echo
  echo "Arguments:"
  echo "  DIRECTORY               Directory to search in. Defaults to current directory."
  echo
  echo "Example usage:"
  echo "  $0 -r '.*\.cs'                    Search for .cs files, excluding defaults."
  echo "  $0 -v '.*\.log'                  Exclude files with .log extension."
  echo "  $0 -n -r 'Member\.cs'             Display only file names matching Member.cs."
  echo "  $0 -r 'jwt|name' -v 'pem'         Include jwt or name, exclude pem."
  echo "  $0 -e                             Include additional identity-related files."
  echo "  $0 -a                             Include all files, no exclusions."
  echo "  $0 -h                             Display this help message."
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
      include_regex="$2"
      shift 2
      ;;
    -v|--invert)
      exclude_regex="$2"
      shift 2
      ;;
    -e|--extended)
      extended=true
      shift
      ;;
    -a|--all)
      exclude_pattern='^$'  # Match nothing (effectively include all)
      shift
      ;;
    -h|--help)
      display_help
      ;;
    -* )
      echo "Unknown option: $1"
      exit 1
      ;;
    * )
      directory="$1"
      shift
      ;;
  esac
done

cd "$directory" || { echo "Failed to change directory to $directory"; exit 1; }

# Define default exclusion pattern in readable format
exclude_pattern='(
    \.git|
    published|
    node_module|
    logs|
    docs|
    bin|
    out|
    obj|
    PlaywrightTests|
    asset|
    migration|
    \.sh$|
    launchSett|
    \.txt$|
    \.md$|
    \.css$|
    \.mjs$|
    \.env$|
    aspnet.lan|
    Dockerfile|
    package-lock|
    package.json|
    wwwroot|
    \.ps1$
)'

# Remove whitespace/newlines for grep
exclude_pattern=$(echo "$exclude_pattern" | tr -d '\n ')

# Extended files (include Identity)
if [ "$extended" = true ]; then
  include_regex="Member\.cs|ApplicationDbContext\.cs|DbInitializer\.cs|Program\.cs|Members\.cshtml|Members\.cshtml\.cs|_Layout\.cshtml|_LoginPartial\.cshtml|EmailSender\.cs|Identity|Account"
fi

# Build the find and filter pipeline
pipeline="find . -type f | egrep -vi \"$exclude_pattern\""

# Apply include filter if set
if [[ -n "$include_regex" ]]; then
  pipeline+=" | egrep -i \"$include_regex\""
fi

# Apply exclude filter if set
if [[ -n "$exclude_regex" ]]; then
  pipeline+=" | egrep -vi \"$exclude_regex\""
fi

# Execute pipeline and process files
eval "$pipeline" | while IFS= read -r f; do
  if [ "$name_only" = true ]; then
    echo "$f"
  else
    echo "$f"
    echo "----------------"
    cat "$f"
    echo "----------------"
  fi
done
