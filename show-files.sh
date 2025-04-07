#!/bin/bash

name_only=false
directory="."
regex_filter=""
extended=false

# Function to display help message
display_help() {
  echo "Usage: $0 [OPTIONS] [DIRECTORY]"
  echo
  echo "Options:"
  echo "  -n, --name-only         Display only the file names, not their content."
  echo "  -r, --regex PATTERN     Filter files using the provided regex pattern."
  echo "  -e, --extended          Include additional identity-related files."
  echo "  -a, --all               Include all files without default exclusions."
  echo "  -h, --help              Display this help message."
  echo
  echo "Arguments:"
  echo "  DIRECTORY               Directory to search in. Defaults to current directory."
  echo
  echo "Example usage:"
  echo "  $0 -r '.*\.cs'                   Search for .cs files, excluding defaults."
  echo "  $0 -n -r 'Member\.cs'            Display only file names matching Member.cs"
  echo "  $0 -e                           Include additional identity-related files."
  echo "  $0 -a                           Include all files, no exclusions."
  echo "  $0 -h                           Display this help message."
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

# Define exclusion pattern in readable format
exclude_pattern='(
    \/.git|
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
    localhost|
    Dockerfile|
    package-lock|
    package.json|
    wwwroot|
    \.ps1$
)'

# Remove newlines and spaces for grep compatibility
exclude_pattern=$(echo "$exclude_pattern" | tr -d '\n ')

# Now use it in grep or other commands
grep -Ev "$exclude_pattern" your_file


# Remove newlines and spaces for grep compatibility
exclude_pattern=$(echo "$exclude_pattern" | tr -d '\n ')
# Extended files (include Identity)
if [ "$extended" = true ]; then
  regex_filter="Member\.cs|ApplicationDbContext\.cs|DbInitializer\.cs|Program\.cs|Members\.cshtml|Members\.cshtml\.cs|_Layout\.cshtml|_LoginPartial\.cshtml|EmailSender\.cs|Identity|Account"
fi
# Generate and safely process file list
if [[ -n "$regex_filter" ]]; then
  find . -type f | egrep -vi "$exclude_pattern" | egrep -i "$regex_filter" | while IFS= read -r f; do
    if [ "$name_only" = true ]; then
      echo "$f"
    else
      echo "$f"
      echo "----------------"
      cat "$f"
      echo "----------------"
    fi
  done
else
  find . -type f | egrep -vi "$exclude_pattern" | while IFS= read -r f; do
    if [ "$name_only" = true ]; then
      echo "$f"
    else
      echo "$f"
      echo "----------------"
      cat "$f"
      echo "----------------"
    fi
  done
fi
