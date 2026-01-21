# Release notes

## 1.0.0-preview (Jan 21, 2026)

### Breaking changes

* Migrated from .NET Framework `4.5` to .NET `8.0`.
* Minimum supported version is now .NET `8.0`.
* `ClientCredential` class removed - replaced with `EdgeGridCredentials`.
* Removed the `EdgeGridV1Signer` class and replaced it with `EdgeGridSigner`.
* Removed the legacy `WebRequest`-based API. Now, it uses `HttpRequestMessage`/`HttpClient` exclusively.
* Removed old signing and request execution methods: `Execute()`, `GetAuthHeader()` signature changes.
* Changed the environment variable naming convention:
  * The default section now uses `AKAMAI_HOST` instead of `AKAMAI_DEFAULT_HOST`.
  * Custom sections use the `AKAMAI_{SECTION}_*` pattern.
* Changed the `SignRequest()` method signature. Now, it takes `EdgeGridCredentials` instead of individual parameters.
* Made redirect handling transparent by default via the `CreateHttpClient()` factory method.
* Changed the manual `HttpClientHandler` configuration for redirect handling. Now, it uses `EdgeGridRedirectHandler`.
* Changed the synchronous `Send()` method behavior in redirect handler. Now, it automatically signs requests.

### Improvements

* Added support for `headers_to_sign`. It allows specifying custom headers to include in a signature.
* Added the `max_body` configuration with the default of `131072` bytes for request body signing.
* Added an automatic `User-Agent` header with library version information.
* Implemented proper header canonicalization for security.
* Added support for URL path parameters (semicolon-separated parameters).
* Added automatic redirect handling with request resigning, via the `EdgeGridSigner.CreateHttpClient()` factory method.
* Enabled nullable reference types across entire codebase for improved null safety.
* Optimized string building operations using `StringBuilder`.
* Implemented proper `IDisposable` resource management to be used with statements.
* Added static compiled regular expressions for better performance.
* Introduced named constants for magic numbers.
* Removed all unused directives.
* Updated to Apache License 2.0.

### Features

* `EdgeGridSigner.CreateHttpClient()`. The factory method that creates an `HttpClient` with automatic redirect handling and resigning.
* `EdgeGridRedirectHandler`. A low-level `DelegatingHandler` for advanced scenarios requiring custom configuration.
* Provided a full feature parity with Python EdgeGrid implementation `v2.0.3`.
* Added a comprehensive test suite.
