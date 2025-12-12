#!/bin/bash

# Akamai EdgeGrid Auth - NuGet Publishing Script
# This script publishes the package to NuGet.org with optional signing

set -e

echo "=========================================="
echo "Akamai EdgeGrid Auth - NuGet Publishing"
echo "=========================================="
echo ""

# Check if package file and API key are provided
if [ -z "$1" ] || [ -z "$2" ]; then
    echo "Usage: ./publish-nuget.sh <package-file> <api-key> [--sign <cert-path> <timestamp-url>]"
    echo ""
    echo "Basic usage:"
    echo "  ./publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.1.0.0.nupkg YOUR_API_KEY"
    echo ""
    echo "With signing:"
    echo "  ./publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.1.0.0.nupkg YOUR_API_KEY \\"
    echo "    --sign /path/to/certificate.pfx http://timestamp.server.com"
    echo ""
    echo "To get your API key:"
    echo "  1. Sign in to https://www.nuget.org"
    echo "  2. Go to Account Settings > API Keys"
    echo "  3. Create or copy your API key"
    echo ""
    exit 1
fi

PACKAGE_FILE=$1
API_KEY=$2
SIGN_CERT=""
TIMESTAMP_URL=""
NUGET_SOURCE="https://api.nuget.org/v3/index.json"

# Parse optional signing parameters
if [ "$3" = "--sign" ]; then
    if [ -z "$4" ] || [ -z "$5" ]; then
        echo "Error: --sign requires certificate path and timestamp URL"
        echo "Usage: --sign <cert-path> <timestamp-url>"
        exit 1
    fi
    SIGN_CERT=$4
    TIMESTAMP_URL=$5
fi

# Verify package file exists
if [ ! -f "$PACKAGE_FILE" ]; then
    echo "Error: Package file not found: $PACKAGE_FILE"
    exit 1
fi

# Verify signing certificate exists if specified
if [ -n "$SIGN_CERT" ] && [ ! -f "$SIGN_CERT" ]; then
    echo "Error: Certificate file not found: $SIGN_CERT"
    exit 1
fi

# Extract version from filename
VERSION=$(basename "$PACKAGE_FILE" | sed 's/Akamai.EdgeGrid.Auth.\(.*\).nupkg/\1/')

echo "Package: $PACKAGE_FILE"
echo "Version: $VERSION"
echo "Target: $NUGET_SOURCE"

if [ -n "$SIGN_CERT" ]; then
    echo "Signing: Enabled (Certificate: $(basename $SIGN_CERT))"
fi
echo ""

# Step 1: Sign the package if requested
if [ -n "$SIGN_CERT" ]; then
    echo "Step 1: Signing package..."
    echo "Certificate: $SIGN_CERT"
    echo "Timestamp URL: $TIMESTAMP_URL"
    echo ""
    
    if ! dotnet nuget sign "$PACKAGE_FILE" \
        --certificate-path "$SIGN_CERT" \
        --timestamper "$TIMESTAMP_URL"; then
        echo "Error: Failed to sign package"
        exit 1
    fi
    echo ""
    echo "✓ Package signed successfully"
    echo ""
fi

# Step 2: Confirmation prompt
echo "Ready to publish version $VERSION to NuGet? (yes/no): " | tr -d '\n'
read -r CONFIRM
echo ""

if [[ ! $CONFIRM =~ ^[Yy][Ee][Ss]$ ]]; then
    echo "Publishing cancelled."
    exit 0
fi

# Step 3: Check if package with this version already exists
echo "Checking if version $VERSION already exists on NuGet..."
RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "https://api.nuget.org/v3-flatcontainer/akamai.edgegrid.auth/$VERSION/akamai.edgegrid.auth.$VERSION.nupkg")

if [ "$RESPONSE" = "200" ]; then
    echo "Error: Version $VERSION already exists on NuGet.org"
    echo "Please use a different version number."
    exit 1
fi

echo "✓ Version $VERSION is available"
echo ""

# Step 4: Push to NuGet
echo "Publishing package to NuGet..."
echo ""

if dotnet nuget push "$PACKAGE_FILE" \
    --api-key "$API_KEY" \
    --source "$NUGET_SOURCE" \
    --skip-duplicate; then
    echo ""
    echo "=========================================="
    echo "✓ Package Published Successfully"
    echo "=========================================="
    echo ""
    echo "Version: $VERSION"
    echo "Package URL: https://www.nuget.org/packages/Akamai.EdgeGrid.Auth/$VERSION"
    
    if [ -n "$SIGN_CERT" ]; then
        echo "Status: Signed and Published"
    else
        echo "Status: Published (Unsigned)"
        echo ""
        echo "Note: Consider signing future releases for increased trust and security."
    fi
    echo ""
    echo "Next steps:"
    echo "1. Verify the package is available on NuGet.org"
    echo "2. Create a GitHub release for v$VERSION"
    echo "3. Announce the release"
    echo ""
else
    echo ""
    echo "Error: Failed to publish package"
    exit 1
fi
