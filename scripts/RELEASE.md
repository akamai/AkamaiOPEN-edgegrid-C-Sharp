# Release guide: Akamai EdgeGrid Auth library

This guide provides instructions for preparing and publishing releases of the Akamai EdgeGrid Auth library to NuGet.

## Overview

The release process consists of two main steps:

1. **Preparation:** Build, test, and package the release.
2. **Publishing:** Upload the package to NuGet.org.

## Prerequisites

- .NET 8.0 SDK installed.
- Git access to the repository.
- NuGet.org account with publish permissions.
- NuGet API key (obtained from NuGet.org account settings).

## Release preparation

### Step 1: Prepare the release

Run the preparation script with the desired version number.

```bash
./prepare-release.sh 2.1.0
```

The script will:

- ✓ Run all unit tests.
- ✓ Update the version in `EdgeGridAuth.csproj`.
- ✓ Clean previous builds.
- ✓ Build in release mode.
- ✓ Create the NuGet package.
- ✓ Verify the package.

**Example output:**
```
==========================================
Akamai EdgeGrid Auth - Release Preparation
==========================================

Preparing release for version: 2.1.0
Step 1: Running tests...
✓ All tests passed

Step 2: Updating version in EdgeGridAuth.csproj...
✓ Version updated to 2.1.0

Step 3: Cleaning previous builds...
✓ Cleaned

Step 4: Building in Release mode...
✓ Release build complete

Step 5: Creating NuGet package...
✓ Package created

Step 6: Verifying package...
✓ Package verified: /path/to/nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg (11K)

==========================================
Release Preparation Complete
==========================================

Version: 2.1.0
Package: /path/to/nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg

Next steps:
1. Review the package contents
2. Create a Git tag: git tag -a v2.1.0 -m "Release version 2.1.0"
3. Push the tag: git push origin v2.1.0
4. Run: ./publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg YOUR_API_KEY
```

### Step 2: Git tag and push (optional but recommended)

Create and push a git tag for the release.

```bash
# Create an annotated tag
git tag -a v2.1.0 -m "Release version 2.1.0"

# Push the tag to the repository
git push origin v2.1.0
```

## Publishing to NuGet

### Step 1: Obtain the NuGet API key

1. Sign in to https://www.nuget.org.
2. Go to **Account Settings** > **API Keys**.
3. Create or copy your API key.
4. Keep it safe and don't commit it to the repository.

### Step 2a: Obtain a code signing certificate (optional but recommended)

For official Akamai packages, signing is **strongly recommended** to ensure package authenticity and integrity.

**Options**

You can request a code signing certificate from one of these:

- Your organization's IT/Security department.
- A certificate authority (CA), such as:
  - DigiCert
  - GlobalSign
  - Sectigo/Comodo
  - SSL.com

**Certificate requirements:**

- Must be in `.pfx` format (PKCS#12).
- Must not be self-signed (NuGet.org rejects self-signed certificates).

**Register the certificate on NuGet.org:**

1. Export the certificate to `.cer` format (DER binary).
2. Sign in to NuGet.org.
3. Go to **Account Settings** > **Certificates**.
4. Click **Register new**.
5. Upload the `.cer` file.

### Step 2b: Publish without signing (unsigned)

Run the publishing script without signing.

```bash
./publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg YOUR_API_KEY
```

### Step 2c: Publish with signing (recommended)

Run the publishing script with signing enabled.

```bash
./publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg YOUR_API_KEY \
  --sign /path/to/certificate.pfx http://timestamp.server.com
```

**Parameters:**

- `--sign`. Enables package signing.
- `/path/to/certificate.pfx`. The path to your code signing certificate.
- `http://timestamp.server.com`. The timestamp authority URL (provided by your certificate authority).

**Common timestamp servers:**

- DigiCert: `http://timestamp.digicert.com`
- GlobalSign: `http://timestamp.globalsign.com`
- Sectigo: `http://timestamp.sectigo.com`

**Example output:**
```
==========================================
Akamai EdgeGrid Auth - NuGet Publishing
==========================================

Package: nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg
Version: 2.1.0
Target: https://api.nuget.org/v3/index.json

Ready to publish version 2.1.0 to NuGet? (yes/no): yes

Checking if version 2.1.0 already exists on NuGet...
✓ Version 2.1.0 is available

Publishing package to NuGet...

Pushing Akamai.EdgeGrid.Auth 2.1.0 to 'https://api.nuget.org/v3/index.json'...
PUT https://www.nuget.org/api/v2/package/
Authenticating...
✓ Package published successfully

==========================================
✓ Package Published Successfully
==========================================

Version: 2.1.0
Package URL: https://www.nuget.org/packages/Akamai.EdgeGrid.Auth/2.1.0

Next steps:
1. Verify the package is available on NuGet.org
2. Create a GitHub Release for v2.1.0
3. Announce the release
```

## Complete release workflow

### Unsigned release (simple)

Here is a complete example of releasing version 2.1.0 without signing.

```bash
# 1. Prepare the release
./prepare-release.sh 2.1.0

# 2. Create Git tag
git tag -a v2.1.0 -m "Release version 2.1.0"
git push origin v2.1.0

# 3. Publish to NuGet (unsigned)
./publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg YOUR_API_KEY

# 4. Create a GitHub Release
# - Go to https://github.com/akamai/AkamaiOPEN-edgegrid-C-Sharp/releases
# - Click "Draft a new release"
# - Select tag: v2.1.0
# - Add release notes from CHANGELOG.md
# - Publish release
```

### Signed release (recommended for official packages)

Here is a complete example of releasing version 2.1.0 with signing.

```bash
# 1. Prepare the release
./prepare-release.sh 2.1.0

# 2. Create Git tag
git tag -a v2.1.0 -m "Release version 2.1.0"
git push origin v2.1.0

# 3. Publish to NuGet (with signing)
./publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg YOUR_API_KEY \
  --sign /path/to/akamai-codesign.pfx http://timestamp.digicert.com

# 4. Create a GitHub Release
# - Go to https://github.com/akamai/AkamaiOPEN-edgegrid-C-Sharp/releases
# - Click "Draft a new release"
# - Select tag: v2.1.0
# - Add release notes from CHANGELOG.md
# - Mention: "Package is cryptographically signed for authenticity"
# - Publish release
```

## Version numbering

Follow [semantic versioning](https://semver.org/):

- **MAJOR** – Incompatible API changes (breaking changes).
- **MINOR** – New functionality in a backward-compatible manner.
- **PATCH** – Bug fixes and backward-compatible changes.

Examples:

- `1.0.0` – Initial release
- `1.1.0` – New features added
- `1.1.1` – Bug fixes
- `2.0.0` – New breaking changes release

## Package signing (recommended)

### Why sign packages

- **Authenticity:** Proves the package comes from Akamai.
- **Integrity:** Ensures the package hasn't been tampered with.
- **Consumer trust:** Developers can verify package authenticity.
- **Industry standard:** Best practice for official/enterprise libraries.

### How it works

1. **Sign the package:** Use a code signing certificate to cryptographically sign the `.nupkg` file.
2. **Register the certificate:** Register the certificate on NuGet.org (one-time setup).
3. **Verify the signature:** Consumers can verify the package signature using NuGet tools.

### Setup (one-time)

1. **Obtain a code signing certificate**

   ```bash
   # Option 1: From your organization's certificate authority
   # Option 2: Purchase from a trusted CA (DigiCert, GlobalSign, SSL.com, etc.)
   ```

2. **Export Certificate to `.cer` format**

   ```bash
   # On Windows
   certutil -encode certificate.pfx certificate.cer
   ```

3. **Register on NuGet.org**

   - Sign in to https://www.nuget.org.
   - **Account Settings** > **Certificates**.
   - Click **Register new**.
   - Upload the `.cer` file.

### Publish with signing

After one-time setup, publish the signed packages.

```bash
./publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg YOUR_API_KEY \
  --sign /path/to/certificate.pfx http://timestamp.digicert.com
```

---

1. ✓ Update `CHANGELOG.md` with changes for this version.
2. ✓ Review all changes since the last release.
3. ✓ Ensure all tests pass.
4. ✓ Update documentation if needed.
5. ✓ Verify the version is correctly set.

## Troubleshooting

### Package already exists

If you get an error that the version already exists:

- You must use a different version number.
- Check NuGet.org to confirm the version is already published.

### Authentication failed

If authentication fails:

- Verify your API key is correct.
- Ensure the API key hasn't expired.
- Check that your account has push permissions.

### Build failed

If the build fails:

- Ensure all tests pass: `dotnet test`.
- Check that .NET 8.0 SDK is installed: `dotnet --version`.
- Clean and rebuild: `rm -rf EdgeGridAuth/bin EdgeGridAuth/obj`.

## Post-release

After successfully publishing:

1. ✓ Verify on NuGet.org that the package is available.
2. ✓ Create a GitHub Release with release notes.
3. ✓ Update documentation if needed.
4. ✓ Announce the release to users.
5. ✓ Update any version references in `README.md`.

## Additional resources

- [NuGet documentation](https://docs.microsoft.com/nuget/)
- [Semantic versioning](https://semver.org/)
- [GitHub Releases](https://help.github.com/articles/creating-releases/)
- [Akamai EdgeGrid Auth – README](../README.md)
- [Changelog](../CHANGELOG.md)
