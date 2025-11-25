# EdgeGridSigner (for .NET/c#)

This library assists in the interaction with Akamai APIs using the EdgeGrid signing method.
API specs can be found at: https://techdocs.akamai.com/

## Project organization
* /EdgeGridAuth - core auth signer project
* /OpenAPI - generic Windows tool to demonstrate using the signer 
* /Akamai.EdgeGrid.Auth.sln - root VisualStudio solution

## Install
* Open the Akamai.EdgeGrid.Auth.sln in Visual Studio; Rebuild All
* Copy the Akamai.EdgeGrid.Auth.dll to your application or solution. 

## Getting Started
* Create an instance of the `EdgeGridCredentials` class (see below for details)
* Create an instance of the `EdgeGridV2Signer` and call either `Sign()` (if you wish to provider a `HttpRequestMessage`)
* or call `GetAuthHeader()` to provide the request elements directly.

For example:
```c#
using Akamai.EdgeGrid.Auth;

EdgeGridV2Signer signer = new EdgeGridV2Signer();
EdgeGridCredentials credential = new EdgeGridCredentials();

Uri uri = new Uri($"https://{credentials.Host}/papi/v1/contracts");
HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("get"), uri);
signer.Sign(httpcredentialRequest, credential);
```

alternatively, you can use the GetAuthHeader() method to construct the request without using HttpRequestMessage
```c#
using Akamai.EdgeGrid.Auth;

EdgeGridV2Signer signer = new EdgeGridV2Signer();
EdgeGridCredentials credential = new EdgeGridCredentials();

string AuthHeader = GetAuthHeader(credential, "get", "/papi/v1/contracts");
# Add AuthHeader value to your request
```

## Loading Credentials

The EdgeGridCredentials class is configured to read Akamai credentials from either Environment Variables or a EdgeRCFile.

### Environment Variables

Environment variables are checked first, and the variables below are checked for their corresponding authentication elements

- client_token - AKAMAI_CLIENT_TOKEN
- client_secret - AKAMAI_CLIENT_SECRET
- host - AKAMAI_HOST
- access_token - AKAMAI_ACCESS_TOKEN
- account_key - AKAMAI_ACCOUNT_KEY

> Note: The `account_key` is only commonly used by Akamai internal staff, so if you've never seen it, don't worry about it.

If you specify a `section` parameter when instantiating the EdgeGridCredentials class, the environment variables checked will difer in the form `AKAMAI_<SECTION>_<ELEMENT>`, e.g. if you specify a section of `appsec`, the following variables will be checked:

- AKAMAI_APPSEC_CLIENT_TOKEN
- AKAMAI_APPSEC_CLIENT_SECRET
- AKAMAI_APPSEC_HOST
- AKAMAI_APPSEC_ACCESS_TOKEN
- AKAMAI_APPSEC_ACCOUNT_KEY

### EdgeRC File

If environment variables cannot be found, or you specify an EdgeRCFile in the constructor, credentials will be read from a file, whose format is expected to be:

```
[default]
client_secret = C113nt53KR3TN6N90yVuAgICxIRwsObLi0E67/N8eRN=
host = akab-h05tnam3wl42son7nktnlnnx-kbob3i3v.luna.akamaiapis.net
access_token = akab-acc35t0k3nodujqunph3w7hzp7-gtm6ij
client_token = akab-c113ntt0k3n4qtari252bfxxbsl-yvsdj
```

If you do not specify otherwise, the default location of the file is `~/.edgerc` and the section is `default`.

## Advanced Features

### Redirect Handling with Automatic Resigning

The library supports automatic redirect following with request resigning. When an API returns a redirect (3xx status), the `EdgeGridRedirectHandler` will automatically:
- Follow the redirect
- Resign the new request with fresh EdgeGrid authentication headers
- Preserve request headers and content

```c#
using Akamai.EdgeGrid.Auth;
using System.Net.Http;

var credentials = new EdgeGridCredentials("~/.edgerc", "default");
var redirectHandler = new EdgeGridRedirectHandler(credentials, maxRedirects: 10);

using var client = new HttpClient(redirectHandler);
var request = new HttpRequestMessage(HttpMethod.Get, "https://akaa-baseurl.luna.akamaiapis.net/api/endpoint");

var signer = new EdgeGridV2Signer();
signer.Sign(request, credentials);

var response = await client.SendAsync(request);
```

See [REDIRECT_HANDLING.md](REDIRECT_HANDLING.md) for detailed documentation and examples.

### Custom Headers in Signature

You can include specific headers in the authentication signature:

```c#
// Via .edgerc file
[default]
client_secret = C113nt53KR3TN6N90yVuAgICxIRwsObLi0E67/N8eRN=
host = akab-h05tnam3wl42son7nktnlnnx-kbob3i3v.luna.akamaiapis.net
access_token = akab-acc35t0k3nodujqunph3w7hzp7-gtm6ij
client_token = akab-c113ntt0k3n4qtari252bfxxbsl-yvsdj
headers_to_sign = X-Custom-Header,X-Another-Header
max_body = 131072
```

```c#
// Or programmatically
var credentials = new EdgeGridCredentials(
    host: "akab-baseurl.luna.akamaiapis.net",
    clientToken: "akab-client-token-xxx",
    clientSecret: "client-secret-xxx",
    accessToken: "akab-access-token-xxx",
    headersToSign: new List<string> { "X-Custom-Header", "X-Another-Header" },
    maxBody: 131072
);
```

## Sample application (EdgeGridConsole.exe)
* A sample application has been created that can take command line parameters.

```
Usage: EdgeGridConeols <-e edgerc-file> <-s section> <-a account-switch-key>
           [-d data] [-f srcfile]
           [-o outfile]
           [-m max-size]
           [-X method]
           [-H header-line]
           [-T content-type]
           <url>

Where:
    -o outfile      local file name to use to save response from the API
    -d data         string of data to PUT to the API
    -f srcfile      local file used as source when action=upload
    -H header-line  Http Header 'Name: value'
    -X method       force HTTP PUT,POST,DELETE
    -T content-type the HTTP content type (default = application/json)
    url             fully qualified api url such as https://akab-1234.luna.akamaiapis.net/diagnostic-tools/v1/locations
```

Example:

```shell
EdgeGridConsole.exe -e ~/.edgerc -s default /edge-diagnostics/v1/edge-locations
```

