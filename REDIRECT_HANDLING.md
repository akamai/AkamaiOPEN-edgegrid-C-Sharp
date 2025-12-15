# EdgeGrid redirect handling

The EdgeGrid library supports automatic redirect handling with request resigning.

## Quick start (recommended)

### Automatic signing and redirect handling

```csharp
using Akamai.EdgeGrid.Auth;
using System.Net.Http;

var credentials = new EdgeGridCredentials("~/.edgerc", "default");

// Create an HttpClient with automatic signing and redirect handling
using var client = EdgeGridSigner.CreateHttpClient(credentials);

var request = new HttpRequestMessage(HttpMethod.Get, 
    $"https://{credentials.Host}/api/endpoint");

// Requests are automatically signed and redirects are followed with resigning
var response = await client.SendAsync(request);
```

`EdgeGridAuth` automatically handles signing and redirects via response hooks.

## Basic usage

### Without automatic signing (manual signing)

```csharp
using Akamai.EdgeGrid.Auth;
using System.Net.Http;

var credentials = new EdgeGridCredentials("~/.edgerc", "default");
var signer = new EdgeGridSigner();

using var client = new HttpClient();
var request = new HttpRequestMessage(HttpMethod.Get, 
    $"https://{credentials.Host}/api/endpoint");

// Manually sign the request
signer.Sign(request, credentials);

var response = await client.SendAsync(request);
// Note: Redirects will NOT be followed or resigned
```

### With manual `EdgeGridRedirectHandler` configuration

```csharp
using Akamai.EdgeGrid.Auth;
using System.Net.Http;

var credentials = new EdgeGridCredentials("~/.edgerc", "default");

// Create the redirect handler with your credentials
// This automatically signs initial requests and handles redirects
var redirectHandler = new EdgeGridRedirectHandler(credentials, maxRedirects: 10);

// Create an HttpClient with the redirect handler
using var client = new HttpClient(redirectHandler);

var request = new HttpRequestMessage(HttpMethod.Get, 
    $"https://{credentials.Host}/api/endpoint");

// No manual signing needed. The handler will automatically:
// 1. Sign the initial request with EdgeGrid auth headers.
// 2. Follow redirects (301, 302, 303, 307, 308).
// 3. Resign each redirected request with new auth headers.
// 4. Preserve request headers and content.
var response = await client.SendAsync(request);
```

## Advanced configuration

### Custom `HttpClientHandler` with redirect support

```csharp
var credentials = new EdgeGridCredentials("~/.edgerc", "default");

// Method 1: Using the factory method with default configuration
using var client = EdgeGridSigner.CreateHttpClient(credentials, maxRedirects: 5);

// Method 2: Custom base handler for advanced scenarios
var baseHandler = new HttpClientHandler
{
    AllowAutoRedirect = false, // Important: disable automatic redirects
    UseCookies = false,
    // Add other configurations as needed
};

// Wrap with the EdgeGrid redirect handler (includes auto-signing)
var redirectHandler = new EdgeGridRedirectHandler(credentials, maxRedirects: 10)
{
    InnerHandler = baseHandler
};

using var customClient = new HttpClient(redirectHandler);

var request = new HttpRequestMessage(HttpMethod.Get, 
    $"https://{credentials.Host}/api/endpoint");

// No manual signing needed – redirectHandler signs automatically
var response = await customClient.SendAsync(request);
```

## How it works

The `EdgeGridRedirectHandler` is a `DelegatingHandler` that:

1. **Signs initial request:** Automatically adds the EdgeGrid auth headers to the initial request.
2. **Intercepts responses:** Checks if the response is a redirect (3xx status codes).
3. **Creates a new request:** Builds a new request for the redirect location.
4. **Preserves context:** Copies headers and content from the original request.
5. **Resigns request:** Signs the new request with fresh EdgeGrid auth headers.
6. **Follows chain:** Continues following redirects up to the maximum limit.
7. **Returns a final response:** This happens when a non-redirect response is received.

## Supported redirect status codes

- **301** – Moved Permanently
- **302** – Found
- **303** – See Other
- **307** – Temporary Redirect
- **308** – Permanent Redirect

## Error handling

### Maximum redirects eceeded

```csharp
try
{
    var response = await client.SendAsync(request);
}
catch (InvalidOperationException ex) when (ex.Message.Contains("Maximum number of redirects"))
{
    // Handle too many redirects (possible redirect loop)
    Console.WriteLine("Redirect limit exceeded");
}
```

## Notes

- The handler automatically disables built-in redirect following to ensure proper resigning.
- Each redirected request gets a fresh timestamp and nonce in the authorization header.
- Request content is preserved for `POST`/`PUT`/`PATCH` methods during redirects.
- The original `Host` header is updated for each redirect location.
