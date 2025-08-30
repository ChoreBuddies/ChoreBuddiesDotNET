#!/bin/bash

echo "Setting up cross-platform Git hooks..."

# Configure Git to use custom hooks path
git config core.hooksPath .githooks

# Make hook files executable
chmod +x .githooks/pre-commit

echo "Git hooks configured successfully!"
echo "Hooks path: $(git config core.hooksPath)"

read -p "Press enter to end"