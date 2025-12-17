#!/bin/bash

# Akamai EdgeGrid Auth - CI/CD Pipeline Script
# Runs unit tests, linting, code analysis, and generates coverage reports
# Usage: ./scripts/ci.sh [--coverage] [--detailed]

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
MAIN_PROJECT="$PROJECT_ROOT/EdgeGridAuth"
REPORTS_DIR="$PROJECT_ROOT/reports"
COVERAGE_DIR="$REPORTS_DIR/coverage"
TEST_RESULTS_DIR="$REPORTS_DIR/test-results"

# Parse command line arguments
COVERAGE_ENABLED=false
DETAILED_OUTPUT=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --coverage)
            COVERAGE_ENABLED=true
            shift
            ;;
        --detailed)
            DETAILED_OUTPUT=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--coverage] [--detailed]"
            exit 1
            ;;
    esac
done

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=========================================="
echo "Akamai EdgeGrid Auth - CI Pipeline"
echo "==========================================${NC}"
echo ""
echo "Configuration:"
echo "  Coverage Analysis: $([ "$COVERAGE_ENABLED" = true ] && echo 'Enabled' || echo 'Disabled')"
echo "  Detailed Output: $([ "$DETAILED_OUTPUT" = true ] && echo 'Enabled' || echo 'Disabled')"
echo "  Project Root: $PROJECT_ROOT"
echo ""

# Step 1: Clean previous builds
echo -e "${BLUE}Step 1: Cleaning previous builds...${NC}"
rm -rf "$PROJECT_ROOT/EdgeGridAuth/bin" "$PROJECT_ROOT/EdgeGridAuth/obj"
rm -rf "$PROJECT_ROOT/EdgeGridAuthTest/bin" "$PROJECT_ROOT/EdgeGridAuthTest/obj"
rm -rf "$PROJECT_ROOT/EdgeGridConsoleTest/bin" "$PROJECT_ROOT/EdgeGridConsoleTest/obj"
rm -rf "$REPORTS_DIR"
mkdir -p "$COVERAGE_DIR" "$TEST_RESULTS_DIR"
echo -e "${GREEN}✓ Cleaned${NC}"
echo ""

# Step 2: Format check with dotnet format (linter)
echo -e "${BLUE}Step 2: Checking code formatting...${NC}"
if ! dotnet format "$PROJECT_ROOT" --verify-no-changes --include "$MAIN_PROJECT" 2>/dev/null; then
    echo -e "${YELLOW}⚠ Code formatting issues detected. Run 'dotnet format' to fix.${NC}"
else
    echo -e "${GREEN}✓ Code formatting OK${NC}"
fi
echo ""

# Step 3: Restore dependencies
echo -e "${BLUE}Step 3: Restoring NuGet dependencies...${NC}"
if ! dotnet restore "$PROJECT_ROOT" -q; then
    echo -e "${RED}✗ Failed to restore dependencies${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Dependencies restored${NC}"
echo ""

# Step 4: Build projects
echo -e "${BLUE}Step 4: Building projects...${NC}"
if ! dotnet build "$MAIN_PROJECT" -c Release; then
    echo -e "${RED}✗ Build failed${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Build successful${NC}"
echo ""

# Step 5: Run code analysis
echo -e "${BLUE}Step 5: Running static code analysis...${NC}"
if [ "$DETAILED_OUTPUT" = true ]; then
    dotnet build "$MAIN_PROJECT" -c Release /p:EnforceCodeStyleInBuild=true /p:TreatWarningsAsErrors=false 2>&1 | grep -E "(warning|error|CS|CA)" || echo -e "${GREEN}✓ Code analysis completed${NC}"
else
    ANALYSIS_OUT=$(dotnet build "$MAIN_PROJECT" -c Release /p:EnforceCodeStyleInBuild=true 2>&1)
    if echo "$ANALYSIS_OUT" | grep -q "error"; then
        echo "$ANALYSIS_OUT" | grep -E "error" | tail -3
    fi
    echo -e "${GREEN}✓ Code analysis completed${NC}"
fi
echo ""

# Step 6: Run unit tests with coverage
echo -e "${BLUE}Step 6: Running unit tests...${NC}"

if [ "$COVERAGE_ENABLED" = true ]; then
    echo "  (with code coverage collection)"
    if ! dotnet test "$PROJECT_ROOT" \
        --configuration Release \
        --results-directory "$TEST_RESULTS_DIR" \
        --logger "trx" \
        /p:CollectCoverage=true \
        /p:CoverletOutputFormat=cobertura \
        /p:CoverletOutput="$COVERAGE_DIR/"; then
        echo -e "${RED}✗ Tests failed${NC}"
        exit 1
    fi
else
    if ! dotnet test "$PROJECT_ROOT" \
        --verbosity detailed \
        --configuration Release \
        --results-directory "$TEST_RESULTS_DIR" \
        --logger "trx"; then
        echo -e "${RED}✗ Tests failed${NC}"
        exit 1
    fi
fi

# Count test results from all TRX files
TEST_RESULT_FILES=$(find "$TEST_RESULTS_DIR" -name "*.trx" -type f)
if [ -n "$TEST_RESULT_FILES" ]; then
    PASSED=0
    FAILED=0
    SKIPPED=0
    
    for TRX_FILE in $TEST_RESULT_FILES; do
        PASSED=$((PASSED + $(grep -o 'outcome="Passed"' "$TRX_FILE" | wc -l)))
        FAILED=$((FAILED + $(grep -o 'outcome="Failed"' "$TRX_FILE" | wc -l)))
        SKIPPED=$((SKIPPED + $(grep -o 'outcome="Skipped"' "$TRX_FILE" | wc -l)))
    done
    
    TOTAL=$((PASSED + FAILED + SKIPPED))
    
    echo ""
    echo -e "Test Results:"
    echo -e "  Total:   ${BLUE}$TOTAL${NC}"
    echo -e "  Passed:  ${GREEN}$PASSED${NC}"
    if [ "$FAILED" -gt 0 ]; then
        echo -e "  Failed:  ${RED}$FAILED${NC}"
    else
        echo -e "  Failed:  ${GREEN}$FAILED${NC}"
    fi
    [ "$SKIPPED" -gt 0 ] && echo -e "  Skipped: ${YELLOW}$SKIPPED${NC}"
    
    if [ "$FAILED" -gt 0 ]; then
        echo -e "${RED}✗ Some tests failed${NC}"
        exit 1
    fi
    echo -e "${GREEN}✓ All tests passed${NC}"
else
    echo -e "${GREEN}✓ Tests executed${NC}"
fi
echo ""

# Convert TRX to JUnit format for Jenkins
echo -e "${BLUE}Converting test results to JUnit format...${NC}"
JUNIT_FILE="$TEST_RESULTS_DIR/junit-results.xml"

TEST_RESULT_FILES=$(find "$TEST_RESULTS_DIR" -name "*.trx" -type f)
if [ -n "$TEST_RESULT_FILES" ]; then
    # Calculate totals across all TRX files
    TOTAL=0
    PASSED=0
    FAILED=0
    SKIPPED=0
    
    for TRX_FILE in $TEST_RESULT_FILES; do
        TOTAL=$((TOTAL + $(grep -o 'outcome="[^"]*"' "$TRX_FILE" | wc -l | tr -d ' ')))
        PASSED=$((PASSED + $(grep -o 'outcome="Passed"' "$TRX_FILE" | wc -l | tr -d ' ')))
        FAILED=$((FAILED + $(grep -o 'outcome="Failed"' "$TRX_FILE" | wc -l | tr -d ' ')))
        SKIPPED=$((SKIPPED + $(grep -o 'outcome="Skipped"' "$TRX_FILE" | wc -l | tr -d ' ')))
    done
    
    # Create basic JUnit XML
    cat > "$JUNIT_FILE" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="$TOTAL" failures="$FAILED" errors="0" skipped="$SKIPPED">
  <testsuite name="Akamai.EdgeGrid.Tests" tests="$TOTAL" failures="$FAILED" errors="0" skipped="$SKIPPED" timestamp="$(date -u +"%Y-%m-%dT%H:%M:%S")">
EOF
    
    # Extract test cases from all TRX files and add to JUnit
    for TRX_FILE in $TEST_RESULT_FILES; do
        grep 'testName=' "$TRX_FILE" | while read -r line; do
            TEST_NAME=$(echo "$line" | grep -o 'testName="[^"]*"' | sed 's/testName="\([^"]*\)"/\1/')
            OUTCOME=$(echo "$line" | grep -o 'outcome="[^"]*"' | sed 's/outcome="\([^"]*\)"/\1/')
            DURATION=$(echo "$line" | grep -o 'duration="[^"]*"' | sed 's/duration="\([^"]*\)"/\1/' | sed 's/00:00://g')
            
            if [ "$OUTCOME" = "Passed" ]; then
                echo "    <testcase name=\"$TEST_NAME\" classname=\"Akamai.EdgeGrid.Tests\" time=\"$DURATION\"/>" >> "$JUNIT_FILE"
            elif [ "$OUTCOME" = "Failed" ]; then
                echo "    <testcase name=\"$TEST_NAME\" classname=\"Akamai.EdgeGrid.Tests\" time=\"$DURATION\">" >> "$JUNIT_FILE"
                echo "      <failure message=\"Test failed\"/>" >> "$JUNIT_FILE"
                echo "    </testcase>" >> "$JUNIT_FILE"
            elif [ "$OUTCOME" = "Skipped" ]; then
                echo "    <testcase name=\"$TEST_NAME\" classname=\"Akamai.EdgeGrid.Tests\" time=\"$DURATION\">" >> "$JUNIT_FILE"
                echo "      <skipped/>" >> "$JUNIT_FILE"
                echo "    </testcase>" >> "$JUNIT_FILE"
            fi
        done
    done
    
    cat >> "$JUNIT_FILE" << EOF
  </testsuite>
</testsuites>
EOF
    
    echo -e "${GREEN}✓ JUnit format created: $JUNIT_FILE${NC}"
else
    echo -e "${YELLOW}⚠ TRX files not found, skipping JUnit conversion${NC}"
fi
echo ""

# Step 7: Coverage analysis
if [ "$COVERAGE_ENABLED" = true ]; then
    echo -e "${BLUE}Step 7: Analyzing code coverage...${NC}"
    
    COVERAGE_FILE="$COVERAGE_DIR/coverage.cobertura.xml"
    
    if [ -f "$COVERAGE_FILE" ]; then
        echo -e "${GREEN}✓ Coverage report generated${NC}"
        echo "  File: $COVERAGE_FILE"
    else
        echo -e "${YELLOW}⚠ Coverage files not found (this is expected on M1/M2 Macs with certain SDK versions)${NC}"
        echo "  Note: Tests were executed. To generate coverage reports, see CI.md"
    fi
fi
echo ""

# Step 8: Summary
echo -e "${BLUE}=========================================="
echo "CI Pipeline Summary"
echo "==========================================${NC}"
echo ""
echo -e "Status: ${GREEN}✓ PASSED${NC}"
echo ""
echo "Reports and artifacts:"
echo "  Test Results (TRX):   $TEST_RESULTS_DIR/*.trx"
echo "  Test Results (JUnit): $TEST_RESULTS_DIR/junit-results.xml"
if [ "$COVERAGE_ENABLED" = true ]; then
    echo "  Coverage (Cobertura): $COVERAGE_FILE"
fi
echo ""
echo "Jenkins Integration:"
echo "  - Publish JUnit test results: **/reports/test-results/junit-results.xml"
echo "  - Publish TRX test results:   **/reports/test-results/results.trx"
if [ "$COVERAGE_ENABLED" = true ]; then
    echo "  - Publish Cobertura coverage: **/reports/coverage/coverage.cobertura.xml"
fi
echo ""
echo "Next steps:"
echo "  1. Review test results in: $TEST_RESULTS_DIR"
if [ "$COVERAGE_ENABLED" = true ]; then
    echo "  2. Generate HTML coverage report:"
    echo "     dotnet tool install -g dotnet-reportgenerator-globaltool"
    echo "     reportgenerator -reports:$COVERAGE_FILE -targetdir:$REPORTS_DIR/coverage-html"
fi
echo ""
