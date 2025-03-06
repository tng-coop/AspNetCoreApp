#!/usr/bin/env bash

if [ -z "$1" ]; then
  echo "Usage: $0 <URL>"
  exit 1
fi

google-chrome --headless --remote-debugging-port=9222 >/dev/null 2>&1 &
CHROME_PID=$!

cleanup() {
  echo "Stopping Chrome (PID $CHROME_PID)..."
  kill $CHROME_PID >/dev/null 2>&1
}

trap cleanup EXIT INT TERM

# Wait for Chrome debugging port to be ready without using sleep
until curl -s http://localhost:9222/json/version >/dev/null 2>&1; do
  sleep 0.1
done

node fetch-html.mjs "$1"
