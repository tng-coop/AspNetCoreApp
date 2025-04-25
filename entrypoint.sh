#!/usr/bin/env bash
set -e

# 1) start your Blazor app in the background
dotnet BlazorWebApp.dll &
app_pid=$!

echo "Waiting for server on port 8080…"

# 2) loop until /dev/tcp/localhost:8080 responds
until bash -c ">/dev/tcp/localhost/8080" >/dev/null 2>&1; do
  sleep 1
done

# 3) once it’s up, echo
echo "hello"

# 4) keep the container alive
wait $app_pid
