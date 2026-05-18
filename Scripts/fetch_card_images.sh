#!/bin/bash

# Ensure jq is installed
if ! command -v jq &> /dev/null; then
    echo "Error: 'jq' is required but not installed."
    exit 1
fi

# Define variables
LOCALE="en_US"
JSON_DIR="../data/hearthstone_data/${LOCALE}"    # Change this to the directory containing your JSON files
MAIN_DIR="../resources/card_images/${LOCALE}"    # Directory where images will be downloaded

# Check if the JSON directory exists
if [ ! -d "$JSON_DIR" ]; then
    echo "Error: Directory '$JSON_DIR' not found!"
    exit 1
fi

# Create the main directory
mkdir -p "$MAIN_DIR"
echo "Created main directory: $MAIN_DIR"

# Loop through all JSON files in the target directory
for JSON_FILE in "$JSON_DIR"/*.json; do
    
    # Check if the file actually exists (handles the case where no .json files exist)
    [ -e "$JSON_FILE" ] || { echo "No JSON files found in $JSON_DIR"; break; }

    echo "Reading JSON file: $JSON_FILE"

    # Parse the JSON and loop through each card in the "cards" array
    jq -c '.cards[]' "$JSON_FILE" | while read -r card; do
        
        # Extract data using jq
        id=$(echo "$card" | jq -r '.id')
        image_url=$(echo "$card" | jq -r '.image')
        crop_url=$(echo "$card" | jq -r '.cropImage')

        # Skip if ID is missing or null
        if [ "$id" == "null" ] || [ -z "$id" ]; then
            continue
        fi

        # Create the specific directory for this Card ID
        CARD_DIR="$MAIN_DIR/$id"
        mkdir -p "$CARD_DIR"

        echo "  Processing Card ID: $id..."

        # Download the full image
        if [ "$image_url" != "null" ] && [ -n "$image_url" ]; then
            filename=$(basename "$image_url")
            curl -s -L "$image_url" -o "$CARD_DIR/full_$filename"
            echo "    -> Downloaded full image"
        fi

        # Download the cropped image
        if [ "$crop_url" != "null" ] && [ -n "$crop_url" ]; then
            crop_filename=$(basename "$crop_url")
            curl -s -L "$crop_url" -o "$CARD_DIR/crop_$crop_filename"
            echo "    -> Downloaded crop image"
        fi

    done

done

echo "All downloads from all JSON files completed successfully!"