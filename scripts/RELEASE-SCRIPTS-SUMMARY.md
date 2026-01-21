# Release scripts and documentation

## Summary

Three new files have been created to facilitate the release and publishing process:

### 1. **prepare-release.sh** (executable script)

An automated script that prepares the library for release:

**Features:**

- ✓ Validates semantic version format.
- ✓ Runs all unit tests (54+ tests).
- ✓ Updates version in `EdgeGridAuth.csproj`.
- ✓ Cleans previous builds.
- ✓ Builds in release mode.
- ✓ Creates the NuGet package (`.nupkg`).
- ✓ Verifies package contents.
- ✓ Provides next steps for git tagging and publishing.

**Usage:**

```bash
./prepare-release.sh 1.0.0
./prepare-release.sh 1.1.0
./prepare-release.sh 1.0.0-beta
```

**Location:** `<project-root-directory>/prepare-release.sh`

---

### 2. **publish-nuget.sh** (executable script)

An automated script that publishes the package to NuGet.org:

**Features:**

- ✓ Validates the package file existence.
- ✓ Checks if the version already exists on NuGet.
- ✓ Confirms the release before publishing.
- ✓ Uploads to NuGet.org using the API key.
- ✓ Provides the package URL and next steps.
- ✓ Handles errors gracefully.

**Usage:**

```bash
./publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.1.0.0.nupkg YOUR_API_KEY
```

**Location:** `<project-root-directory>/publish-nuget.sh`

---

### 3. **RELEASE.md** (documentation)

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

**Location:** `<project-root-directory>/RELEASE.md`

---

## Quick start guide

### For preparation:

```bash
cd <project-root-directory>
bash prepare-release.sh 1.0.0
```

### For publishing:
```bash
cd <project-root-directory>
bash publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.1.0.0.nupkg YOUR_NUGET_API_KEY
```

---

## Files modified

1. **EdgeGridAuth.csproj**
   - Added NuGet package metadata (PackageId, Version, Title, Authors, etc.).
   - Set up for package publishing.

2. **Akamai.EdgeGrid.Auth.nuspec** (New)
   - Created the `.nuspec` file for detailed package configuration.
   - Includes DLL and PDB file definitions.
   - Detailed release notes and breaking changes.

---

## Current package status

- **Version:** 1.0.0
- **Location:** `nupkg/Akamai.EdgeGrid.Auth.1.0.0.nupkg` (11 KB)
- **Tests:** 54/54 passing ✓
- **Ready for:** Ownership transfer and NuGet publishing

---

## Next steps

1. **Get Official Akamai NuGet account**
   - Create or verify Akamai organization account on NuGet.org.

2. **Request ownership transfer**
   - Send an email to the NuGet support (use the template from `RELEASE.md`).
   - Request transfer from the current owner (andreqb) or direct ownership.

3. **Generate a NuGet API key**
   - Sign in to NuGet.org.
   - Go to **Account Settings** > **API Key**s.
   - Generate a new API key.

4. **Publish the package**
   - Run: `bash publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.1.0.0.nupkg YOUR_API_KEY`.

5. **Verify on NuGet**
   - Check https://www.nuget.org/packages/Akamai.EdgeGrid.Auth/1.0.0.

6. **Create GitHub Release**
   - Tag the release: `git tag -a v1.0.0 -m "Release version 1.0.0"`.
   - Push the tag: `git push origin v1.0.0`.
   - Create a GitHub release with notes from `CHANGELOG.md`.

---

## Release workflow diagram

```
┌─────────────────────────────────────────┐
│ prepare-release.sh <version>            │ Run the preparation script
│ - Validate the version                  │
│ - Run tests                             │
│ - Update the version                    │
│ - Build the release                     │
│ - Create .`nupkg`                       │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ Git tagging                             │
│ git tag -a v<version> -m "..."          │
│ git push origin v<version>              │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ publish-nuget.sh <package> <api-key>    │ Run the publishing script
│ - Validate the package                  │
│ - Check if the version exists           │
│ - Confirm with a user                   │
│ - Upload to NuGet.org                   │
│ - Provide the package URL               │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│ Post-release                            │
│ - Verify on NuGet.org                   │
│ - Create a GitHub Release               │
│ - Announce the release                  │
└─────────────────────────────────────────┘
```

---

## Important notes

- Always run tests before releasing.
- Use semantic versioning (MAJOR.MINOR.PATCH).
- Update `CHANGELOG.md` before each release.
- Never hardcode API keys in scripts or version control.
- Keep the NuGet API key secure.

---

For more details, see **RELEASE.md**
