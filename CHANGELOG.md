# Changelog

All notable changes to this project will be documented in this file.

## Unreleased

## 2.0.0 (2025-11-25)

### Breaking changes

• Migrated from .NET Framework 4.5 to .NET 8.0.
• Minimum supported version is now .NET 8.0.

### Improvements

• Added `headers_to_sign` support - allows specifying custom headers to include in signature.
• Added `max_body` configuration with default of 131072 bytes for request body signing.
• Added automatic User-Agent header with library version information.
• Implemented proper header canonicalization for security.
• Added redirect handling with automatic request resigning via `EdgeGridRedirectHandler`.
• Added support for URL path parameters (semicolon-separated parameters).
• Enabled nullable reference types across entire codebase for improved null safety.
• Optimized string building operations using `StringBuilder`.
• Implemented proper `IDisposable` resource management with using statements.
• Added static compiled regular expressions for better performance.
• Introduced named constants for magic numbers.
• Removed all unused using directives.
• Updated to Apache License 2.0 matching Python implementation.

### Features

• `EdgeGridRedirectHandler` - DelegatingHandler for automatic redirect following with resigning.
• Full feature parity with Python EdgeGrid implementation v2.0.3.
• Comprehensive test suite with 36 tests covering all functionality.
