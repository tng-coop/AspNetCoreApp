#!/bin/bash

name_only=false

# Parse arguments
while [[ "$#" -gt 0 ]]; do
  case $1 in
    -n|--name-only) name_only=true ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
  shift
done

FILES=$(find . -type f | egrep -vi '(./git|./published|node_module|logs|docs|./bin|./out|/obj|/.git|PlaywrightTests|asset|migratio|\.sh$|launchSett|\.txt$|\.md$|\.css$|\.mjs$|\.env$|localhost|Dockerfile|package-lock|package.json|\.ps1$)')

for f in $FILES
do
  if [ "$name_only" = true ]; then
    echo $f
  else
    echo $f
    echo "----------------"
    cat $f
    echo "----------------"
  fi
done