#!/bin/bash

# Akamai EdgeGrid Auth - Release Preparation Script
# This script prepares the repository for NuGet publishing

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$SCRIPT_DIR"
PACKAGE_DIR="$PROJECT_ROOT/nupkg"

echo "=========================================="
echo "Akamai EdgeGrid Auth - Release Preparation"
echo "=========================================="
echo ""

# Check if version argument is provided
if [ -z "$1" ]; then
    echo "Usage: ./prepare-release.sh <version>"
    echo "Example: ./prepare-release.sh 1.0.0"
    exit 1
fi

VERSION=$1

# Validate version format (semantic versioning)
if ! [[ $VERSION =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9]+)?$ ]]; then
    echo "Error: Invalid version format. Use semantic versioning (e.g., 1.0.0 or 1.0.0-beta)"
    exit 1
fi

echo "Preparing release for version: $VERSION"
echo ""

# Step 1: Verify tests pass
echo "Step 1: Running tests..."
if ! dotnet test "$PROJECT_ROOT" --configuration Release; then
    echo "Error: Tests failed. Aborting release preparation."
    exit 1
fi
echo "✓ All tests passed"
echo ""

# Step 2: Update version in csproj
echo "Step 2: Updating version in EdgeGridAuth.csproj..."
csproj_file="$PROJECT_ROOT/EdgeGridAuth/EdgeGridAuth.csproj"
sed -i.bak "s/<Version>.*<\/Version>/<Version>$VERSION<\/Version>/g" "$csproj_file"
rm -f "$csproj_file.bak"
echo "✓ Version updated to $VERSION"
echo ""

# Step 3: Clean all build artifacts (after version update)
echo "Step 3: Cleaning all build artifacts..."
rm -rf "$PROJECT_ROOT/EdgeGridAuth/bin" "$PROJECT_ROOT/EdgeGridAuth/obj"
rm -rf "$PROJECT_ROOT/EdgeGridAuthTest/bin" "$PROJECT_ROOT/EdgeGridAuthTest/obj"
rm -rf "$PROJECT_ROOT/EdgeGridConsole/bin" "$PROJECT_ROOT/EdgeGridConsole/obj"
rm -rf "$PROJECT_ROOT/EdgeGridConsoleTest/bin" "$PROJECT_ROOT/EdgeGridConsoleTest/obj"
echo "✓ Cleaned"
echo ""

# Step 4: Build and test in Release mode
echo "Step 4: Building and testing in Release mode..."
if ! dotnet test "$PROJECT_ROOT" --configuration Release; then
    echo "Error: Build or tests failed. Aborting release preparation."
    exit 1
fi
echo "✓ Build and tests passed"
echo ""

# Step 5: Create NuGet package
echo "Step 5: Creating NuGet package..."
mkdir -p "$PACKAGE_DIR"
rm -f "$PACKAGE_DIR"/*.nupkg
if ! dotnet pack "$PROJECT_ROOT/EdgeGridAuth" -c Release -o "$PACKAGE_DIR"; then
    echo "Error: Package creation failed. Aborting release preparation."
    exit 1
fi
echo "✓ Package created"
echo ""

# Step 6: Verify package
echo "Step 6: Verifying package..."
PACKAGE_FILE="$PACKAGE_DIR/Akamai.EdgeGrid.Auth.$VERSION.nupkg"
if [ ! -f "$PACKAGE_FILE" ]; then
    echo "Error: Package file not found at $PACKAGE_FILE"
    exit 1
fi

PACKAGE_SIZE=$(ls -lh "$PACKAGE_FILE" | awk '{print $5}')
echo "✓ Package verified: $PACKAGE_FILE ($PACKAGE_SIZE)"
echo ""

# Step 7: Display package contents
echo "Step 7: Package contents:"
unzip -l "$PACKAGE_FILE" | head -20
echo ""

# Step 8: Generate summary
echo "=========================================="
echo "Release Preparation Complete"
echo "=========================================="
echo ""
echo "Version: $VERSION"
echo "Package: $PACKAGE_FILE"
echo ""
echo "Next steps:"
echo "1. Review the package contents"
echo "2. Create a Git tag: git tag -a v$VERSION -m \"Release version $VERSION\""
echo "3. Push the tag: git push origin v$VERSION"
echo "4. Run: ./publish-nuget.sh $PACKAGE_FILE YOUR_API_KEY"
echo ""
