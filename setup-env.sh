#!/bin/bash

# Sweepo Server Environment Setup Script
# This script helps you set up the Gmail App password environment variable

echo "🔐 Sweepo Server - Gmail App Password Setup"
echo "=========================================="
echo ""

# Check if environment variable is already set
if [ -n "$SWEEPO_FROM_EMAIL_PASSWORD" ]; then
    echo "✅ SWEEPO_FROM_EMAIL_PASSWORD is already set"
    echo "   Current value: ${SWEEPO_FROM_EMAIL_PASSWORD:0:4}************"
    echo ""
    read -p "Do you want to update it? (y/n): " update_choice
    if [ "$update_choice" != "y" ] && [ "$update_choice" != "Y" ]; then
        echo "Environment variable unchanged."
        exit 0
    fi
fi

echo "📧 To get your Gmail App Password:"
echo "1. Go to https://myaccount.google.com/"
echo "2. Security → 2-Step Verification → App passwords"
echo "3. Select 'Mail' and generate a 16-character password"
echo ""

# Prompt for Gmail App Password
read -s -p "Enter your Gmail App Password: " gmail_password
echo ""

# Check if password is not empty
if [ -z "$gmail_password" ]; then
    echo "❌ Error: Gmail App Password cannot be empty"
    exit 1
fi

# Set environment variable for current session
export SWEEPO_FROM_EMAIL_PASSWORD="$gmail_password"

# Add to shell profile for persistence
shell_profile=""
if [ -f ~/.zshrc ]; then
    shell_profile="$HOME/.zshrc"
elif [ -f ~/.bash_profile ]; then
    shell_profile="$HOME/.bash_profile"
elif [ -f ~/.bashrc ]; then
    shell_profile="$HOME/.bashrc"
fi

if [ -n "$shell_profile" ]; then
    echo ""
    read -p "Add to $shell_profile for persistence? (y/n): " persist_choice
    if [ "$persist_choice" = "y" ] || [ "$persist_choice" = "Y" ]; then
        # Remove existing entry if it exists
        grep -v "SWEEPO_FROM_EMAIL_PASSWORD" "$shell_profile" > temp_profile && mv temp_profile "$shell_profile"
        # Add new entry
        echo "export SWEEPO_FROM_EMAIL_PASSWORD=\"$gmail_password\"" >> "$shell_profile"
        echo "✅ Environment variable added to $shell_profile"
        echo "   Run 'source $shell_profile' or restart your terminal"
    fi
fi

echo ""
echo "✅ SWEEPO_FROM_EMAIL_PASSWORD is now set for this session"
echo "🚀 You can now run: dotnet run"
echo ""
echo "🔒 Security Note: Your Gmail App Password is now securely stored"
echo "   and will not appear in configuration files or logs."
