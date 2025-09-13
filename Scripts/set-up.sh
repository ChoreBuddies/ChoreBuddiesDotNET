#!/bin/bash
# Scripts/set-up.sh
# Set up git hooks from the Scripts folder

HOOK_NAME="pre-commit"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
HOOKS_DIR="$SCRIPT_DIR/../.git/hooks"

# Ensure .git/hooks exists
if [ ! -d "$HOOKS_DIR" ]; then
  echo "Error: $HOOKS_DIR does not exist. Are you in a git repository?"
  read -p "Press Enter to exit..."
  exit 1
fi

# Copy pre-commit hook
cp "$SCRIPT_DIR/$HOOK_NAME" "$HOOKS_DIR/$HOOK_NAME"

# Make it executable
chmod +x "$HOOKS_DIR/$HOOK_NAME"

echo "✅ Git hook '$HOOK_NAME' has been set up."

# Wait for user input before exiting
read -p "Press Enter to finish..."
