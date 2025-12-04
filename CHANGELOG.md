# Changelog

All notable changes to this project will be documented in this file.

## Unreleased

## 2.0.0 (2025-11-25)

### Breaking changes

• Migrated from .NET Framework 4.5 to .NET 8.0.
• Minimum supported version is now .NET 8.0.
• `ClientCredential` class removed - replaced with `EdgeGridCredentials`.
• `EdgeGridV1Signer` class removed - replaced with `EdgeGridV2Signer`.
• Removed legacy `WebRequest`-based API - now uses `HttpRequestMessage`/`HttpClient` exclusively.
• Removed old signing and request execution methods (`Execute()`, `GetAuthHeader()` signature changes).
• Environment variable naming convention changed:
  - Default section now uses `AKAMAI_HOST` instead of `AKAMAI_DEFAULT_HOST`
  - Custom sections use `AKAMAI_{SECTION}_*` pattern
• `SignRequest()` method signature changed - now takes `EdgeGridCredentials` instead of individual parameters.
• Redirect handling is now transparent by default via `CreateHttpClient()` factory method.
• Manual `HttpClientHandler` configuration for redirect handling changed - now uses `EdgeGridRedirectHandler`.
• Synchronous `Send()` method behavior changed in redirect handler - now automatically signs requests.

### Improvements

• Added `headers_to_sign` support - allows specifying custom headers to include in signature.
• Added `max_body` configuration with default of 131072 bytes for request body signing.
• Added automatic User-Agent header with library version information.
• Implemented proper header canonicalization for security.
• Added support for URL path parameters (semicolon-separated parameters).
• Added automatic redirect handling with request resigning via `EdgeGridV2Signer.CreateHttpClient()` factory method.
• Enabled nullable reference types across entire codebase for improved null safety.
• Optimized string building operations using `StringBuilder`.
• Implemented proper `IDisposable` resource management with using statements.
• Added static compiled regular expressions for better performance.
• Introduced named constants for magic numbers.
• Removed all unused using directives.
• Updated to Apache License 2.0.

### Features

• `EdgeGridV2Signer.CreateHttpClient()` - Factory method that creates an HttpClient with automatic redirect handling and resigning, where `EdgeGridAuth` automatically handles redirects.
• `EdgeGridRedirectHandler` - Low-level DelegatingHandler for advanced scenarios requiring custom configuration.
• Comprehensive test suite.
