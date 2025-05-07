#!/bin/bash

name_only=false
directory="."
include_regex=""
exclude_regex=""

# Function to display help message
display_help() {
  cat <<EOF
Usage: $0 [OPTIONS] [DIRECTORY]

Options:
  -n, --name-only         Display only the file names, not their content.
  -r, --regex PATTERN     Include files matching the provided regex pattern.
  -v, --invert PATTERN    Exclude files matching the provided regex pattern.
  -a, --all               Include all files without default exclusions.
  -h, --help              Display this help message.

Arguments:
  DIRECTORY               Directory to search in. Defaults to current directory.

Example:
  $0 -r '.*\.cs'      Search for .cs files, excluding defaults.
  $0 -v '.*\.log'     Exclude files with .log extension.
  $0 -n -r Member\.cs Display only file names matching Member.cs.
  $0 -a               Include all files, no exclusions.
EOF
  exit 0
}

# Parse args
while [[ $# -gt 0 ]]; do
  case $1 in
    -n|--name-only) name_only=true; shift ;;
    -r|--regex)      include_regex="$2"; shift 2 ;;
    -v|--invert)     exclude_regex="$2"; shift 2 ;;
    -a|--all)        all=true; shift ;;
    -h|--help)       display_help ;;
    -*)
      echo "Unknown option: $1" >&2
      exit 1
      ;;
    *)
      directory="$1"; shift
      ;;
  esac
done

cd "${directory}" || { echo "Cannot cd to ${directory}"; exit 1; }

# If --all, skip pruning and ext-filters entirely:
if [[ $all ]]; then
  base_find=( find . -type f )
else
  # directories to skip entirely:
  skip_dirs=( .git published node_module logs docs
  BlazorWebApp/obj
  BlazorWebApp.Tests
  Uploader/obj
  Uploader/bin
   BlazorWebApp/bin node_modules bin out obj PlaywrightTests asset migration launchSett wwwroot )
  # file extensions to drop:
  skip_exts=( sh txt md css mjs env ps1 )

  # build the find + prune:
  base_find=( find . )
  for d in "${skip_dirs[@]}"; do
    base_find+=( -path "./$d" -prune -o )
  done
  base_find+=( -type f )

  # drop unwanted extensions:
  for ext in "${skip_exts[@]}"; do
    base_find+=( ! -name "*.$ext" )
  done
fi

# Now convert array to a pipeline string:
pipeline="${base_find[@]}"

# Apply include / exclude regex if given:
if [[ -n $include_regex ]]; then
  pipeline+=" | egrep -i '${include_regex}'"
fi
if [[ -n $exclude_regex ]]; then
  pipeline+=" | egrep -vi '${exclude_regex}'"
fi

# Finally, print or cat:
if $name_only; then
  pipeline+=" | while IFS= read -r f; do echo \"\$f\"; done"
else
  pipeline+=" | while IFS= read -r f; do
    echo \"\$f\"
    echo '----------------'
    cat \"\$f\"
    echo '----------------'
  done"
fi

# Execute it:
eval "$pipeline"
