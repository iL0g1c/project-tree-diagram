#!/bin/bash

# Check if a directory is provided
if [ -z "$1" ]; then
    echo "Usage: $0 <directory>"
    exit 1
fi

# Input directory
INPUT_DIR="$1"

# Output file
OUTPUT_FILE="combined.sql"

# Clear the output file if it exists
> "$OUTPUT_FILE"

# Iterate through all SQL files in the directory
for sql_file in "$INPUT_DIR"/*.sql; do
    if [ -f "$sql_file" ]; then
        # Add the filename as a comment
        echo "-- File: $(basename "$sql_file")" >> "$OUTPUT_FILE"
        echo "" >> "$OUTPUT_FILE"

        # Append the content of the file
        cat "$sql_file" >> "$OUTPUT_FILE"

        # Add a blank line for separation
        echo "" >> "$OUTPUT_FILE"
        echo "" >> "$OUTPUT_FILE"
    fi
done

echo "Combined SQL files are saved in $OUTPUT_FILE"

