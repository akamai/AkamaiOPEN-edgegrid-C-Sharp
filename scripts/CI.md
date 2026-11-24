# CI/CD Configuration Guide

This document explains how to run the EdgeGrid Auth project through a complete CI/CD pipeline with unit tests, linting, coverage analysis, and result reporting.

## Quick Start

The .NET 10 SDK builds and tests `net10.0`. With the .NET 11 SDK and .NET 10 runtime, the pipeline tests the same `net10.0` library assembly on both `net10.0` and `net11.0`. 
Jenkins uses the .NET 10 SDK image directly and builds and tests .NET 10 only, without installing additional runtimes or building the repository Dockerfile. GitHub Actions validates both runtimes.

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
- Generates a separate Cobertura XML report for each test project and target framework
- Reads each test project's selected frameworks from MSBuild and requires a fresh, nonempty report for every project/framework pair: .NET 10 under SDK 10, and both .NET 10 and .NET 11 under SDK 11

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
├── EdgeGridAuthTest/
│   ├── coverage.net10.0.cobertura.xml
│   └── coverage.net11.0.cobertura.xml
└── EdgeGridConsoleTest/
    ├── coverage.net10.0.cobertura.xml
    └── coverage.net11.0.cobertura.xml
```

The .NET 11 reports are generated only when SDK 11 or later is used. Separate project directories prevent one test project's reports from overwriting another's. 
Consumers must include all `reports/coverage/**/*.cobertura.xml` reports.

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
- **Files:** `reports/coverage/**/*.cobertura.xml`
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
reportgenerator "-reports:reports/coverage/**/*.cobertura.xml" \
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
          dotnet-version: |
            10.0.x
            11.0.x
      
      - name: Run CI Pipeline
        run: ./scripts/ci.sh --coverage
      
      - name: Upload Coverage Reports
        uses: codecov/codecov-action@v3
        continue-on-error: true
        with:
          directory: ./reports/coverage
          fail_ci_if_error: false
          flags: unittests
          name: codecov-umbrella
```

Codecov uploads are non-blocking while using the unauthenticated legacy v3 uploader, whose shared rate limit can reject otherwise valid uploads. Make uploads blocking only after migrating to a supported uploader, configuring authentication for trusted runs, and defining an explicit fork-PR policy. The CI script still fails if coverage reports are missing or empty.

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
  dotnetVersion: '11.0.x'

steps:
  - task: UseDotNet@2
    inputs:
      packageType: 'runtime'
      version: '10.0.x'

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
  
  - task: PublishCodeCoverageResults@2
    inputs:
      summaryFileLocation: '**/reports/coverage/**/*.cobertura.xml'
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
                coberturaAdapter('**/reports/coverage/**/*.cobertura.xml')
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
1. Review all reports under `reports/coverage/`
2. Use HTML reporter to visualize uncovered lines
3. Add tests for uncovered code paths

### Build Fails
1. Run `dotnet clean` to remove cached builds
2. Run `dotnet restore` to refresh dependencies
3. Check for C# syntax errors
4. Verify the .NET 10 or .NET 11 SDK is installed: `dotnet --version`. To test both runtimes, use SDK 11 and check that both the .NET 10 and .NET 11 runtimes are listed by `dotnet --list-runtimes`.

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
