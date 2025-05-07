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
EOF
  exit 0
}

# Parse args
while [[ $# -gt 0 ]]; do
  case $1 in
    -n|--name-only)   name_only=true; shift ;;
    -r|--regex)       include_regex="$2"; shift 2 ;;
    -v|--invert)      exclude_regex="$2"; shift 2 ;;
    -a|--all)         all=true; shift ;;
    -h|--help)        display_help ;;
    -*)
      echo "Unknown option: $1" >&2
      exit 1
      ;;
    *)
      directory="$1"; shift ;;
  esac
done

cd "${directory}" || { echo "Cannot cd to ${directory}"; exit 1; }

if [[ $all ]]; then
  base_find=( find . -type f )
else
  # explicit paths to skip
  skip_dirs=(
    .git published node_module logs docs
    BlazorWebApp/obj BlazorWebApp.Tests
    Uploader/obj Uploader/bin
    BlazorWebApp/bin node_modules bin out obj
    PlaywrightTests asset migration launchSett wwwroot
  )
  # extensions to drop
  skip_exts=( sh txt md css mjs env ps1 )

  # build the grouped-prune clause
  prune_args=()
  for d in "${skip_dirs[@]}"; do
    prune_args+=( -path "./$d" -o )
  done
  # ← new: prune any directory named "dist"
  prune_args+=( -type d -name dist -o )
  # remove trailing -o
  prune_args=( "${prune_args[@]:0:${#prune_args[@]}-1}" )

  # assemble find: prune-clause OR file-clause
  base_find=( find . '\(' "${prune_args[@]}" '\)' -prune -o -type f )

  # drop unwanted extensions
  for ext in "${skip_exts[@]}"; do
    base_find+=( ! -name "*.$ext" )
  done

  # only print from the file-branch
  base_find+=( -print )
fi

# build the rest of the pipeline
pipeline="${base_find[@]}"
if [[ -n $include_regex ]]; then
  pipeline+=" | egrep -i '${include_regex}'"
fi
if [[ -n $exclude_regex ]]; then
  pipeline+=" | egrep -vi '${exclude_regex}'"
fi

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

eval "$pipeline"
