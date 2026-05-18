#!/bin/bash

# Exit immediately if a command exits with a non-zero status
set -euo pipefail

# --- Configuration ---
# Replace these with your actual Battle.net API credentials
CLIENT_ID=$1
CLIENT_SECRET=$2
REGION="us"    # Options: us, eu, kr, tw
LOCALE="en_US" # e.g., en_US, uk_UA (if supported by the specific endpoint)
OUT_DIR="../data/hearthstone_data/${LOCALE}"

# Allow passing pageSize as an argument, default to 300
PAGE_SIZE=${3:-300}

# --- Functions ---

get_access_token() {
  local token_response
  token_response=$(curl -s -u "${CLIENT_ID}:${CLIENT_SECRET}" \
    -d grant_type=client_credentials \
    "https://oauth.battle.net/token")

  # Extract token using jq
  local token
  token=$(echo "$token_response" | jq -r '.access_token')

  if [[ "$token" == "null" || -z "$token" ]]; then
    echo "Error: Failed to authenticate. Check your Client ID and Secret." >&2
    exit 1
  fi

  echo "$token"
}

fetch_all_pages() {
  local token="$1"
  local current_page=1
  local total_pages=1 # Will be updated after the first request

  if [[ -d "$OUT_DIR" ]]; then
    echo "Directory '$OUT_DIR' already exists. Cleaning up old files..."
    # The :? is a safety measure. If OUT_DIR is accidentally empty or unset,
    # it throws an error instead of running rm -rf /*
    rm -rf "${OUT_DIR:?}"/*
  fi
  mkdir -p "$OUT_DIR"

  echo "Starting card sync..."
  echo "Target Page Size: $PAGE_SIZE"

  while [[ "$current_page" -le "$total_pages" ]]; do
    echo "Fetching page $current_page..."

    local response
    response=$(
      curl -s -G -H "Authorization: Bearer ${token}" "https://${REGION}.api.blizzard.com/hearthstone/cards" \
        --data-urlencode "locale=${LOCALE}" \
        --data-urlencode "pageSize=${PAGE_SIZE}" \
        --data-urlencode "page=${current_page}"
    )

    # On the first iteration, extract the total pageCount from the API response
    if [[ "$current_page" -eq 1 ]]; then
      total_pages=$(echo "$response" | jq -r '.pageCount')

      if [[ "$total_pages" == "null" || -z "$total_pages" ]]; then
        echo "Error: Could not retrieve page count. Response might be malformed or rate-limited." >&2
        echo "API Response: $response" >&2
        exit 1
      fi
      echo "Total pages to fetch: $total_pages"
    fi

    # Save the structured JSON to your output directory
    local output_file="${OUT_DIR}/cards_page_${current_page}.json"
    echo "$response" | jq '.' >"$output_file"

    current_page=$((current_page + 1))

    # Polite throttling to avoid hitting API rate limits
    sleep 0.5
  done

  echo "Data successfully pulled and saved to ./$OUT_DIR/"
}

# --- Main Execution ---

main() {
  if [[ "$CLIENT_ID" == "YOUR_CLIENT_ID" ]]; then
    echo "Please update the script with your Battle.net CLIENT_ID and CLIENT_SECRET."
    exit 1
  fi

  echo "Requesting access token..."
  TOKEN=$(get_access_token)

  fetch_all_pages "$TOKEN"
}

main
