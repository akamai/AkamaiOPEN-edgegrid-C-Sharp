# NuGet package signing guide

## Overview

Package signing provides cryptographic proof that a NuGet package comes from a trusted source and hasn't been tampered with. This guide explains how to set up and use package signing for the Akamai EdgeGrid Auth library.

## Why sign packages

✓ **Authenticity:** Proves package originates from Akamai.  
✓ **Integrity:** Ensures no tampering occurred.  
✓ **Trust:** Builds confidence for enterprise consumers.  
✓ **Compliance:** Meets security requirements for official packages.  
✓ **Standards:** Industry best practice for official libraries.  

## Prerequisites

- A code signing certificate in `.pfx` format.
- A certificate from a trusted certificate authority (CA).
- The .NET 6.0.100 SDK or later.
- The NuGet.org account with owner privileges.

## Step 1: Obtain a code signing certificate

### Option A: From your organization

If Akamai has an internal certificate authority:

1. Contact your IT/Security team.
2. Request a code signing certificate.
3. Specify: code signing, extended key usage, 2048-bit RSA or stronger.
4. Export as PKCS#12 in `.pfx` format.

### Option B: From a public certificate authority

Purchase from trusted providers:

- **DigiCert** (https://www.digicert.com)
- **GlobalSign** (https://www.globalsign.com)
- **Sectigo** (https://www.sectigo.com)
- **SSL.com** (https://www.ssl.com)

**Certificate requirements:**

- Type: code signing certificate
- Format: PKCS#12 (.pfx)
- Key Size: 2048-bit RSA or stronger
- Validity: At least 1 year

> **Important:** NuGet.org does NOT accept self-signed certificates.

## Step 2: Export a certificate to DER format

To register the certificate on NuGet.org, you need a DER-encoded `.cer` file.

### On Windows (PowerShell)

Export from the certificate file.

```powershell
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2("C:\path\to\certificate.pfx", "password")
[System.IO.File]::WriteAllBytes("C:\path\to\certificate.cer", $cert.RawData)
```

Or use `certutil`:

```cmd
certutil -encode certificate.pfx certificate.cer
```

### On macOS/Linux

```bash
# Extract a public key from PFX
openssl pkcs12 -in certificate.pfx -clcerts -nokeys -out certificate.pem

# Convert to DER format
openssl x509 -in certificate.pem -outform DER -out certificate.cer
```

## Step 3: Register the certificate on NuGet.org

1. **Sign in to NuGet.org**
   - Go to https://www.nuget.org.
   - Sign in with your Akamai account.

2. **Navigate to the certificates**
   - Click your profile icon (top right).
   - Select **Account settings**.
   - Expand the **Certificates** section.

3. **Register the certificate**
   - Click **Register new**.
   - Browse and select your `certificate.cer` file.
   - Click **Upload**.

4. **Verify Registration**
   - The certificate should now appear in your certificates list.
   - Note down the certificate fingerprint (you may need this later).

## Step 4: Sign and publish the package

### Using the `publish-nuget.sh` script

```bash
./publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg YOUR_API_KEY \
  --sign /path/to/certificate.pfx http://timestamp.digicert.com
```

**Parameters:**

- `nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg` – The package file.
- `YOUR_API_KEY` – The NuGet API key.
- `--sign` – Enables signing.
- `/path/to/certificate.pfx` – A path to the certificate file.
- `http://timestamp.digicert.com` – A timestamp service URL.

### Manual signing (without the script)

```bash
# Sign the package
dotnet nuget sign nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg \
  --certificate-path /path/to/certificate.pfx \
  --timestamper http://timestamp.digicert.com

# Publish to NuGet
dotnet nuget push nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

## Timestamp servers

Timestamps are required for signed packages. They ensure signatures remain valid after the certificate expires.

**Common timestamp servers:**

| Provider | URL |
|----------|-----|
| DigiCert | `http://timestamp.digicert.com` |
| GlobalSign | `http://timestamp.globalsign.com` |
| Sectigo | `http://timestamp.sectigo.com` |
| SSL.com | `http://timestamp.ssl.com` |

Contact your certificate provider for their specific timestamp URL.

## Verify package signature

With the NuGet CLI.

```bash
# Verify a signed package
nuget verify nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg
```

With the dotnet CLI.

```bash
# Verify package integrity
dotnet nuget verify nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg
```

On NuGet.org.

Signed packages display a *Signed* badge on the NuGet.org page.

## Troubleshooting

| Error message | Cause | Solution |
| --------------- | -----------| ---------- |
| `Certificate not found` | Incorrect path or password. | Verify the `.pfx` file path and password are correct. |
| `Invalid timestamp` | The timestamp server URL is incorrect or unreachable. | Verify the timestamp server URL with your certificate provider. |
| `Certificate not registered` | The certificate hasn't been registered on NuGet.org. | Complete Step 3 to register the certificate. |
| `Signing failed after certificate registered` | Using a different certificate than what's registered. | Ensure you're signing with the same certificate you registered. |
| `NuGet.org rejected signed package` | The signature is invalid, or the certificate has expired, or there is other validation issue. | Verify the certificate is valid and re-sign with the current timestamp. |

## Best practices

1. **Secure certificate storage**
   - Store `.pfx` files in a secure location.
   - Use strong passwords for `.pfx` files.
   - Never commit certificates to git.
   - Consider using a hardware token for production certificates.

2. **Certificate renewal**
   - Monitor certificate expiration dates.
   - Renew before expiration.
   - Register a new certificate on NuGet.org before using.
   - Keep an old certificate registered for consumers to verify past packages.

3. **Consistent signing**
   - Always sign release packages.
   - Use the same certificate for all releases (for consistency).
   - Include signing status in release notes.

4. **Documentation**
   - Document which certificate is used for which release.
   - Maintain a certificate inventory.
   - Keep the timestamp server information documented.

## Complete release workflow (with signing)

```bash
# 1. Prepare the release
./prepare-release.sh 2.1.0

# 2. Tag the release
git tag -a v2.1.0 -m "Release version 2.1.0"
git push origin v2.1.0

# 3. Sign and publish
./publish-nuget.sh nupkg/Akamai.EdgeGrid.Auth.2.1.0.nupkg YOUR_API_KEY \
  --sign /secure/path/to/certificate.pfx http://timestamp.digicert.com

# 4. Verify on NuGet.org
# - Visit https://www.nuget.org/packages/Akamai.EdgeGrid.Auth/2.1.0
# - Verify "Signed" badge appears

# 5. Create a GitHub Release
# - Tag: v2.1.0
# - Notes: Include "Cryptographically signed for authenticity"
```

## Additional resources

- [NuGet package signing](https://learn.microsoft.com/en-us/nuget/create-packages/sign-a-package)
- [Code signing certificate providers](https://learn.microsoft.com/en-us/security/trusted-root/participants-list)
- [dotnet nuget sign](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-sign)
- [NuGet security](https://learn.microsoft.com/en-us/nuget/concepts/security-best-practices)
