#!/bin/bash

# Ivy-Tendril macOS Installer
# This script installs .NET 10 SDK, GitHub CLI, and Ivy-Tendril.

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== Ivy-Tendril Installer for macOS ===${NC}"

if [[ "$OSTYPE" != "darwin"* ]]; then
    echo -e "${RED}Error: This script is only for macOS.${NC}"
    exit 1
fi

ARCH=$(uname -m)
if [[ "$ARCH" == "arm64" ]]; then
    echo -e "Detected Architecture: Apple Silicon (arm64)"
    GH_ARCH="arm64"
elif [[ "$ARCH" == "x86_64" ]]; then
    echo -e "Detected Architecture: Intel (x64)"
    GH_ARCH="amd64"
else
    echo -e "${RED}Error: Unsupported architecture: $ARCH${NC}"
    exit 1
fi

echo -e "\n${BLUE}Step 1: Checking for .NET 10 SDK...${NC}"
if command -v dotnet &> /dev/null && dotnet --version | grep -q "^10\."; then
    echo -e "${GREEN}✓ .NET 10 SDK is already installed.${NC}"
else
    echo -e "Installing .NET 10 SDK..."
    curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0 --quality preview
    
    # Export for current session
    export DOTNET_ROOT="$HOME/.dotnet"
    export PATH="$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools"
    echo -e "${GREEN}✓ .NET 10 SDK installed successfully.${NC}"
fi

echo -e "\n${BLUE}Step 2: Checking for Git...${NC}"
if command -v git &> /dev/null; then
    echo -e "${GREEN}✓ Git is already installed.${NC}"
else
    echo -e "Installing Git (via Xcode Command Line Tools)..."
    xcode-select --install
    echo -e "${RED}Please complete the GUI installation prompt, then run this script again.${NC}"
    exit 1
fi

echo -e "\n${BLUE}Step 3: Checking for GitHub CLI (gh)...${NC}"
if command -v gh &> /dev/null; then
    echo -e "${GREEN}✓ GitHub CLI is already installed.${NC}"
else
    echo -e "Installing GitHub CLI (gh)..."
    # Get latest version from GitHub
    LATEST_GH=$(curl -sL -o /dev/null -w %{url_effective} https://github.com/cli/cli/releases/latest | grep -oE "[^/]+$")
    GH_VERSION=${LATEST_GH#v}
    
    GH_TEMP=$(mktemp -d)
    GH_ZIP="gh_${GH_VERSION}_macOS_${GH_ARCH}.zip"
    
    echo -e "Downloading gh ${GH_VERSION}..."
    curl -sSL -o "$GH_TEMP/$GH_ZIP" "https://github.com/cli/cli/releases/download/${LATEST_GH}/${GH_ZIP}"
    
    cd "$GH_TEMP"
    unzip -q "$GH_ZIP"
    
    # Install binary
    sudo mkdir -p /usr/local/bin
    sudo mv gh_${GH_VERSION}_macOS_${GH_ARCH}/bin/gh /usr/local/bin/
    
    cd - > /dev/null
    rm -rf "$GH_TEMP"
    echo -e "${GREEN}✓ GitHub CLI installed to /usr/local/bin/gh.${NC}"
fi

echo -e "\n${BLUE}Step 4: Checking for PowerShell (pwsh)...${NC}"
if command -v pwsh &> /dev/null || dotnet tool list -g | grep -qi "powershell"; then
    echo -e "${GREEN}✓ PowerShell is already installed.${NC}"
else
    echo -e "Installing PowerShell (pwsh)..."
    dotnet tool install --global PowerShell
    echo -e "${GREEN}✓ PowerShell installed successfully.${NC}"
fi

echo -e "\n${BLUE}Step 5: Installing Ivy-Tendril...${NC}"
# Use the internal source if provided, otherwise secondary
# We'll try official NuGet first, then fallback to Ivy feed if requested
IVY_SOURCE="https://api.nuget.org/v3/index.json"

if dotnet tool list -g | grep -q "ivy.tendril"; then
    echo -e "Updating Ivy-Tendril..."
    dotnet tool update -g Ivy.Tendril --add-source "$IVY_SOURCE" || true
else
    echo -e "Installing Ivy-Tendril..."
    dotnet tool install -g Ivy.Tendril --add-source "$IVY_SOURCE"
fi

# 6. PATH Configuration
echo -e "\n${BLUE}Step 6: Configuring PATH...${NC}"
DOTNET_TOOLS_PATH="$HOME/.dotnet/tools"
SHELL_PROFILE=""

if [[ "$SHELL" == *"zsh"* ]]; then
    SHELL_PROFILE="$HOME/.zshrc"
elif [[ "$SHELL" == *"bash"* ]]; then
    if [[ -f "$HOME/.bash_profile" ]]; then
        SHELL_PROFILE="$HOME/.bash_profile"
    else
        SHELL_PROFILE="$HOME/.bashrc"
    fi
fi

if [[ -n "$SHELL_PROFILE" ]]; then
    if ! grep -q "DOTNET_ROOT" "$SHELL_PROFILE"; then
        echo -e "Adding .NET environment variables to $SHELL_PROFILE"
        echo -e "\n# .NET SDK & Tools\nexport DOTNET_ROOT=\"\$HOME/.dotnet\"\nexport PATH=\"\$PATH:\$DOTNET_ROOT:\$DOTNET_ROOT/tools\"" >> "$SHELL_PROFILE"
        echo -e "${GREEN}✓ PATH updated. Please restart your terminal or run: source $SHELL_PROFILE${NC}"
    else
        echo -e "${GREEN}✓ PATH and DOTNET_ROOT already configured in $SHELL_PROFILE.${NC}"
    fi
else
    echo -e "${RED}Warning: Could not detect shell profile. Manually add $DOTNET_TOOLS_PATH to your PATH.${NC}"
fi

echo -e "\n${GREEN}=== Installation Complete! ===${NC}"
echo -e "You can now run Ivy-Tendril by typing: ${BLUE}tendril${NC}"
echo -e "To launch the GUI, use: ${BLUE}tendril --photino${NC}"

if ! command -v gh &> /dev/null && ! /usr/local/bin/gh --version &> /dev/null; then
    echo -e "${RED}Note: You may need to restart your terminal for 'gh' to be available.${NC}"
fi

echo -e "\n${BLUE}Try running:${NC} tendril --version"
