# EdgeGrid Redirect Handling

The EdgeGrid library now supports automatic redirect handling with request resigning.

## Basic Usage

### Without Redirect Handling (Default)
```csharp
using Akamai.EdgeGrid.Auth;
using System.Net.Http;

var credentials = new EdgeGridCredentials("~/.edgerc", "default");
var signer = new EdgeGridV2Signer();

using var client = new HttpClient();
var request = new HttpRequestMessage(HttpMethod.Get, "https://akaa-baseurl.luna.akamaiapis.net/api/endpoint");

// Sign the request
signer.Sign(request, credentials);

var response = await client.SendAsync(request);
```

### With Automatic Redirect Handling
```csharp
using Akamai.EdgeGrid.Auth;
using System.Net.Http;

var credentials = new EdgeGridCredentials("~/.edgerc", "default");

// Create the redirect handler with your credentials
var redirectHandler = new EdgeGridRedirectHandler(credentials);

// Optionally set a custom max redirects (default is 10)
// var redirectHandler = new EdgeGridRedirectHandler(credentials, maxRedirects: 5);

// Create HttpClient with the redirect handler
using var client = new HttpClient(redirectHandler);

var request = new HttpRequestMessage(HttpMethod.Get, "https://akaa-baseurl.luna.akamaiapis.net/api/endpoint");

// Sign the initial request
var signer = new EdgeGridV2Signer();
signer.Sign(request, credentials);

// The handler will automatically:
// 1. Follow redirects (301, 302, 303, 307, 308)
// 2. Resign each redirected request with new auth headers
// 3. Preserve request headers and content
var response = await client.SendAsync(request);
```

## Advanced Configuration

### Custom HttpClientHandler with Redirect Support
```csharp
var credentials = new EdgeGridCredentials("~/.edgerc", "default");

// Start with a custom base handler
var baseHandler = new HttpClientHandler
{
    AllowAutoRedirect = false, // Important: disable automatic redirects
    UseCookies = false,
    // Add other configurations as needed
};

// Wrap with EdgeGrid redirect handler
var redirectHandler = new EdgeGridRedirectHandler(credentials, maxRedirects: 10)
{
    InnerHandler = baseHandler
};

using var client = new HttpClient(redirectHandler);

var request = new HttpRequestMessage(HttpMethod.Get, "https://akaa-baseurl.luna.akamaiapis.net/api/endpoint");

var signer = new EdgeGridV2Signer();
signer.Sign(request, credentials);

var response = await client.SendAsync(request);
```

## How It Works

The `EdgeGridRedirectHandler` is a `DelegatingHandler` that:

1. **Intercepts responses**: Checks if the response is a redirect (3xx status codes)
2. **Creates new request**: Builds a new request for the redirect location
3. **Preserves context**: Copies headers and content from the original request
4. **Resigns request**: Automatically signs the new request with fresh EdgeGrid auth headers
5. **Follows chain**: Continues following redirects up to the maximum limit
6. **Returns final response**: Once a non-redirect response is received

## Supported Redirect Status Codes

- **301** - Moved Permanently
- **302** - Found
- **303** - See Other
- **307** - Temporary Redirect
- **308** - Permanent Redirect

## Error Handling

### Maximum Redirects Exceeded
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

## Python Implementation Comparison

This implementation matches the Python library's `handle_redirect` functionality:

**Python:**
```python
def handle_redirect(self, res, **_):
    if res.is_redirect:
        redirect_location = res.headers['location']
        logger.debug("signing the redirected url: %s", redirect_location)
        request_to_sign = res.request.copy()
        request_to_sign.url = redirect_location
        res.request.headers['Authorization'] = self.ah.make_auth_header(
            request_to_sign, eg_timestamp(), new_nonce())
```

**C#:**
The `EdgeGridRedirectHandler` provides equivalent functionality with proper .NET async patterns and HttpClient architecture.

## Notes

- The handler automatically disables built-in redirect following to ensure proper resigning
- Each redirected request gets a fresh timestamp and nonce in the authorization header
- Request content is preserved for POST/PUT/PATCH methods during redirects
- The original `Host` header is updated for each redirect location
