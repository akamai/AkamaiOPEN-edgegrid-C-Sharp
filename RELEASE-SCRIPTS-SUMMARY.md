# Release Scripts and Documentation

## Summary

Three new files have been created to facilitate the release and publishing process:

### 1. **prepare-release.sh** (Executable Script)
An automated script that prepares the library for release:

**Features:**
- ✓ Validates semantic version format
- ✓ Runs all unit tests (54+ tests)
- ✓ Updates version in `EdgeGridAuth.csproj`
- ✓ Cleans previous builds
- ✓ Builds in Release mode
- ✓ Creates NuGet package (.nupkg)
- ✓ Verifies package contents
- ✓ Provides next steps for Git tagging and publishing

**Usage:**
```bash
./prepare-release.sh 2.0.0
./prepare-release.sh 2.1.0
./prepare-release.sh 2.0.0-beta
```

**Location:** `/Users/miwojci/dev/AkamaiOPEN-edgegrid-C-Sharp/prepare-release.sh`

---

### 2. **publish-nuget.sh** (Executable Script)
An automated script that publishes the package to NuGet.org:

**Features:**
- ✓ Validates package file existence
- ✓ Checks if version already exists on NuGet
- ✓ Confirms release before publishing
- ✓ Uploads to NuGet.org using API key
- ✓ Provides package URL and next steps
- ✓ Handles errors gracefully

**Usage:**
```bash
./publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.2.0.0.nupkg YOUR_API_KEY
```

**Location:** `/Users/miwojci/dev/AkamaiOPEN-edgegrid-C-Sharp/publish-nuget.sh`

---

### 3. **RELEASE.md** (Documentation)
Comprehensive guide for the entire release process:

**Sections:**
- Overview of the release process
- Prerequisites
- Step-by-step release preparation
- Git tagging instructions
- NuGet API key setup
- Complete release workflow examples
- Version numbering guidelines
- Troubleshooting guide
- Post-release checklist

**Location:** `/Users/miwojci/dev/AkamaiOPEN-edgegrid-C-Sharp/RELEASE.md`

---

## Quick Start Guide

### For Preparation:
```bash
cd /Users/miwojci/dev/AkamaiOPEN-edgegrid-C-Sharp
bash prepare-release.sh 2.0.0
```

### For Publishing:
```bash
cd /Users/miwojci/dev/AkamaiOPEN-edgegrid-C-Sharp
bash publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.2.0.0.nupkg YOUR_NUGET_API_KEY
```

---

## Files Modified

1. **EdgeGridAuth.csproj**
   - Added NuGet package metadata (PackageId, Version, Title, Authors, etc.)
   - Set up for package publishing

2. **Akamai.EdgeGrid.Auth.nuspec** (New)
   - Created .nuspec file for detailed package configuration
   - Includes DLL and PDB file definitions
   - Detailed release notes and breaking changes

---

## Current Package Status

- **Version:** 2.0.0
- **Location:** `nupkg/Akamai.EdgeGrid.Auth.2.0.0.nupkg` (11 KB)
- **Tests:** 54/54 passing ✓
- **Ready for:** Ownership transfer and NuGet publishing

---

## Next Steps

1. **Get Official Akamai NuGet Account**
   - Create or verify Akamai organization account on NuGet.org

2. **Request Ownership Transfer**
   - Send email to NuGet support (use template from RELEASE.md)
   - Request transfer from current owner (andreqb) or direct ownership

3. **Generate NuGet API Key**
   - Sign in to NuGet.org
   - Go to Account Settings > API Keys
   - Generate new API key

4. **Publish Package**
   - Run: `bash publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.2.0.0.nupkg YOUR_API_KEY`

5. **Verify on NuGet**
   - Check https://www.nuget.org/packages/Akamai.EdgeGrid.Auth/2.0.0

6. **Create GitHub Release**
   - Tag release: `git tag -a v2.0.0 -m "Release version 2.0.0"`
   - Push tag: `git push origin v2.0.0`
   - Create GitHub release with notes from CHANGELOG.md

---

## Release Workflow Diagram

```
┌─────────────────────────────────────────┐
│ prepare-release.sh <version>            │ Run preparation script
│ - Validate version                      │
│ - Run tests                             │
│ - Update version                        │
│ - Build Release                         │
│ - Create .nupkg                         │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ Git Tagging (Optional)                  │
│ git tag -a v<version> -m "..."          │
│ git push origin v<version>              │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ publish-nuget.sh <package> <api-key>    │ Run publishing script
│ - Validate package                      │
│ - Check if version exists               │
│ - Confirm with user                     │
│ - Upload to NuGet.org                   │
│ - Provide package URL                   │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ Post-Release                            │
│ - Verify on NuGet.org                   │
│ - Create GitHub Release                 │
│ - Announce release                      │
└─────────────────────────────────────────┘
```

---

## Important Notes

- Always run tests before releasing
- Use semantic versioning (MAJOR.MINOR.PATCH)
- Update CHANGELOG.md before each release
- Never hardcode API keys in scripts or version control
- Keep NuGet API key secure

---

For more details, see **RELEASE.md**
