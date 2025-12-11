# CI/CD Configuration Guide

This document explains how to run the EdgeGrid Auth project through a complete CI/CD pipeline with unit tests, linting, coverage analysis, and result reporting.

## Quick Start

### Run Basic CI Pipeline
```bash
./scripts/ci.sh
```

### Run with Coverage Analysis
```bash
./scripts/ci.sh --coverage
```

### Run with Detailed Output
```bash
./scripts/ci.sh --detailed
```

### Run with Both
```bash
./scripts/ci.sh --coverage --detailed
```

## Pipeline Stages

The CI pipeline runs the following checks in order:

### 1. **Restore Dependencies**
- Downloads and restores all NuGet packages
- Ensures all references are available

### 2. **Clean Build Artifacts**
- Removes previous build outputs
- Clears old test results and coverage reports
- Starts with a fresh state

### 3. **Code Formatting Check**
- Uses `dotnet format` to verify code style consistency
- Does NOT auto-fix (use `dotnet format` to fix manually)
- Warning if issues found (non-blocking)

### 4. **Build Project**
- Compiles in Release mode
- Enables nullable reference type checking
- Verifies all code compiles correctly

### 5. **Static Code Analysis**
- Runs Roslyn analyzers
- Checks for code style violations
- Identifies potential issues

### 6. **Unit Tests**
- Runs all 54 unit tests using MSTest
- Tests cover:
  - Credential loading (environment variables and .edgerc files)
  - EdgeGrid v2 signing algorithm
  - Redirect handling (sync and async)
  - Content preservation for POST/PUT/PATCH/DELETE
  - Edge cases and error scenarios

### 7. **Code Coverage** (optional with `--coverage`)
- Measures line and branch coverage
- Uses Coverlet for .NET coverage collection
- Generates Cobertura XML report

## Output Locations

### Test Results
```
reports/test-results/
├── results.trx               # Visual Studio Test Results (TRX format)
├── junit-results.xml         # JUnit XML format (Jenkins-compatible)
```

### Coverage Reports
```
reports/coverage/
├── coverage.cobertura.xml    # Cobertura XML format (Jenkins-compatible)
```

## Test Result Formats

The CI pipeline generates test results in multiple formats for maximum compatibility:

### TRX Format (Visual Studio)
- **File:** `reports/test-results/results.trx`
- **Use for:** Visual Studio, Azure DevOps, TFS
- **Contains:** Detailed test execution data, output logs, stack traces

### JUnit XML Format (Jenkins)
- **File:** `reports/test-results/junit-results.xml`
- **Use for:** Jenkins, GitLab CI, CircleCI, most CI/CD systems
- **Contains:** Test names, pass/fail status, execution times

### Cobertura Coverage Format
- **File:** `reports/coverage/coverage.cobertura.xml`
- **Use for:** Jenkins Cobertura plugin, SonarQube, CodeCov
- **Contains:** Line coverage, branch coverage, per-file metrics

## Interpreting Results

### Test Output Example
```
Test Results:
  Total:   54
  Passed:  54
  Failed:   0
  Skipped:  0
```

### Coverage Output Example
```
Code Coverage:
  Line Coverage:   95.3%
  Branch Coverage: 92.1%
```

## Generating HTML Coverage Reports

To create a visual HTML coverage report:

```bash
# Install report generator (one-time)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator -reports:reports/coverage/coverage.cobertura.xml \
                -targetdir:reports/coverage-html

# Open in browser
open reports/coverage-html/index.html
```

## CI/CD Integration

### GitHub Actions Example

Create `.github/workflows/ci.yml`:

```yaml
name: CI Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Run CI Pipeline
        run: ./scripts/ci.sh --coverage
      
      - name: Upload Coverage Reports
        uses: codecov/codecov-action@v3
        with:
          files: ./reports/coverage/coverage.cobertura.xml
          flags: unittests
          name: codecov-umbrella
```

### Azure Pipelines Example

Create `azure-pipelines.yml`:

```yaml
trigger:
  - main
  - develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  dotnetVersion: '8.0.x'

steps:
  - task: UseDotNet@2
    inputs:
      version: $(dotnetVersion)
  
  - task: DotNetCoreCLI@2
    displayName: 'Run CI Pipeline'
    inputs:
      command: 'custom'
      custom: 'bash'
      arguments: './scripts/ci.sh --coverage'
  
  - task: PublishTestResults@2
    inputs:
      testResultsFormat: 'VSTest'
      testResultsFiles: '**/reports/test-results/*.trx'
  
  - task: PublishCodeCoverageResults@1
    inputs:
      codeCoverageTool: 'Cobertura'
      summaryFileLocation: '**/reports/coverage/coverage.cobertura.xml'
```

### Jenkins Pipeline Example

A complete `Jenkinsfile` is provided in the repository root. Key configuration:

```groovy
pipeline {
    agent any
    
    stages {
        stage('Build & Test') {
            steps {
                sh './scripts/ci.sh --coverage'
            }
        }
    }
    
    post {
        always {
            // Publish JUnit test results
            junit testResults: '**/reports/test-results/junit-results.xml'
            
            // Publish Cobertura coverage
            publishCoverage adapters: [
                coberturaAdapter('**/reports/coverage/coverage.cobertura.xml')
            ]
            
            // Archive artifacts
            archiveArtifacts artifacts: 'reports/**/*.xml,reports/**/*.trx'
        }
    }
}
```

**Required Jenkins Plugins:**
- JUnit Plugin (for test results)
- Code Coverage API Plugin (for coverage visualization)
- Cobertura Plugin (for coverage reports)

**Jenkins Test Result Display:**
- **Test Results Tab:** Shows all 54 tests with pass/fail status
- **Test Trend Graph:** Historical test execution trends
- **Coverage Report:** Per-file line and branch coverage metrics

## Troubleshooting

### Tests Fail
1. Check test output in `reports/test-results/`
2. Run specific test for debugging: `dotnet test EdgeGridAuthTest --filter "ClassName.MethodName"`
3. Review test logs for assertion failures

### Low Coverage
1. Review `reports/coverage/coverage.cobertura.xml`
2. Use HTML reporter to visualize uncovered lines
3. Add tests for uncovered code paths

### Build Fails
1. Run `dotnet clean` to remove cached builds
2. Run `dotnet restore` to refresh dependencies
3. Check for C# syntax errors
4. Verify .NET 8.0 SDK is installed: `dotnet --version`

### Format Issues
1. View issues: `dotnet format --verify-no-changes --include EdgeGridAuth`
2. Auto-fix: `dotnet format --include EdgeGridAuth`

## Performance Tips

- **First run**: ~30-45 seconds (includes restore)
- **Subsequent runs**: ~15-20 seconds (cached dependencies)
- **With coverage**: +5-10 seconds for instrumentation
- **On CI server**: May be slower depending on resources

## Best Practices

1. **Pre-commit**: Run locally before pushing
   ```bash
   ./scripts/ci.sh --coverage
   ```

2. **Set Coverage Thresholds**: Use policy enforcement in CI
   - Minimum line coverage: 80%
   - Minimum branch coverage: 75%

3. **Run on Every Push**: Configure webhook to trigger CI

4. **Monitor Trends**: Track coverage metrics over time

5. **Failed Builds**: Fix immediately to keep main branch green

## See Also

- `RELEASE.md` - Release preparation and NuGet publishing
- `PACKAGE-SIGNING.md` - Package signing for releases
- `README.md` - Getting started with the library
