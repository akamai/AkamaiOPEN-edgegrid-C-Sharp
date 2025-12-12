# EdgeGrid for .NET/C#

This library implements an Authentication handler for the Akamai EdgeGrid Authentication scheme in .NET/C#.

You can find all Akamai APIs on [TechDocs](https://techdocs.akamai.com/home/page/apis).

## Project organization

| Folder | Description|
| -------- | --------- |
| `/EdgeGridAuth` | The core auth signer project. |
| `/EdgeGridAuthTest` | The unit tests for the core auth signer project. |
| `/EdgeGridConsole` | The  generic command-line tool to demonstrate using the signer. |
| `/Akamai.EdgeGrid.Auth.sln` | The root project solution. |


## Install

Run `dotnet build` and then `dotnet test` in the root directory.

## Authentication

You can obtain the authentication credentials through an API client. Requests to the API are timestamped, signed, and executed immediately.

1. [Create authentication credentials](https://techdocs.akamai.com/developer/docs/edgegrid).
2. Place your credentials in an EdgeGrid resource file (`.edgerc`) under the `[default]` heading in your local home directory.

    ```
    [default]
    client_secret = C113nt53KR3TN6N90yVuAgICxIRwsObLi0E67/N8eRN=
    host = akab-h05tnam3wl42son7nktnlnnx-kbob3i3v.luna.akamaiapis.net
    access_token = akab-acc35t0k3nodujqunph3w7hzp7-gtm6ij
    client_token = akab-c113ntt0k3n4qtari252bfxxbsl-yvsdj
    ```

### Load credentials

Use the `EdgeGridCredentials` class to load your credentials from either environment variables or an `.edgerc` file.

The library will first check whether your credentials are defined in environment variables. By default, it uses these variables:

* `AKAMAI_CLIENT_TOKEN`
* `AKAMAI_CLIENT_SECRET`
* `AKAMAI_HOST`
* `AKAMAI_ACCESS_TOKEN`
* `AKAMAI_MAX_BODY`
* `AKAMAI_ACCOUNT_KEY`

> **Note:** The `AKAMAI_ACCOUNT_KEY` is used only internally by Akamai staff.

You can define multiple configurations by specifying the credentials' section header as an interfix that you insert between `AKAMAI_` and the credential name. For example, if you pass `appsec` as the credentials' section header in the `section` parameter when instantiating the `EdgeGridCredentials` class, the class will look for the `AKAMAI_APPSEC_CLIENT_TOKEN` environment variable.

If environment variables can't be found, or you specify an `.edgerc` file in the constructor, the credentials will be read from the file. If you don't provide any credentials, the default location of the file is `~/.edgerc`, and the section is `default`.

## Get started

### Basic usage

To make an authenticated request, create an instance of the `EdgeGridCredentials` and pass the path to your `.edgerc` file and the credentials' section header.

Then use the `EdgeGridV2Signer` to create an `HttpClient` with automatic signing and redirect handling.

```c#
using Akamai.EdgeGrid.Auth;
using System.Net.Http;

var credentials = new EdgeGridCredentials("~/.edgerc", "default");

// Create an HttpClient with automatic signing and redirect handling
using var client = EdgeGridV2Signer.CreateHttpClient(credentials);

// Create and send the request – signing is handled automatically
var request = new HttpRequestMessage(HttpMethod.Get,
    $"https://{credentials.Host}/identity-management/v3/user-profile");

var response = await client.SendAsync(request);
```

### Sign your request manually

You can also sign your request manually if you want to have more control over the `HttpClient` configuration.

```c#
using Akamai.EdgeGrid.Auth;

EdgeGridV2Signer signer = new EdgeGridV2Signer();
EdgeGridCredentials credential = new EdgeGridCredentials();

Uri uri = new Uri($"https://{credentials.Host}/identity-management/v3/user-profile");
HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("get"), uri);
signer.Sign(request, credential);

// Use your own HttpClient
using var client = new HttpClient();
var response = await client.SendAsync(request);
```

### Use `GetAuthHeader` directly

Alternatively, you can use the `GetAuthHeader()` method to construct the request without using `HttpRequestMessage`.

```c#
using Akamai.EdgeGrid.Auth;

EdgeGridV2Signer signer = new EdgeGridV2Signer();
EdgeGridCredentials credential = new EdgeGridCredentials();

string AuthHeader = GetAuthHeader(credential, "get", "identity-management/v3/user-profile");
# Add AuthHeader value to your request
```

## Advanced features

### Automatic signing and redirect handling (default behavior)

The library provides automatic request signing and redirect following. For this feature, use the `CreateHttpClient()` factory method.

```c#
using Akamai.EdgeGrid.Auth;
using System.Net.Http;

var credentials = new EdgeGridCredentials("~/.edgerc", "default");

// Create HttpClient with automatic signing and redirect handling
using var client = EdgeGridV2Signer.CreateHttpClient(credentials, maxRedirects: 10);

// Send requests – signing and redirects are handled transparently
var request = new HttpRequestMessage(HttpMethod.Get,
    $"https://{credentials.Host}/identity-management/v3/user-profile");

var response = await client.SendAsync(request);
```

For advanced scenarios requiring custom configuration, you can use `EdgeGridRedirectHandler` directly.

```c#
using Akamai.EdgeGrid.Auth;
using System.Net.Http;

var credentials = new EdgeGridCredentials("~/.edgerc", "default");
var redirectHandler = new EdgeGridRedirectHandler(credentials, maxRedirects: 10)
{
    InnerHandler = new HttpClientHandler
    {
        // Custom configuration here
    }
};

using var client = new HttpClient(redirectHandler);
var request = new HttpRequestMessage(HttpMethod.Get,
    $"https://{credentials.Host}/identity-management/v3/user-profile");

// Send the request – signing is automatic
var response = await client.SendAsync(request);
```

### Custom headers in signature

You can include specific headers in the authentication signature.

* Via an `.edgerc` file.

    ```c#
    [default]
    client_secret = C113nt53KR3TN6N90yVuAgICxIRwsObLi0E67/N8eRN=
    host = akab-h05tnam3wl42son7nktnlnnx-kbob3i3v.luna.akamaiapis.net
    access_token = akab-acc35t0k3nodujqunph3w7hzp7-gtm6ij
    client_token = akab-c113ntt0k3n4qtari252bfxxbsl-yvsdj
    headers_to_sign = X-Custom-Header,X-Another-Header
    max_body = 131072
    ```

* Programmatically.

    ```c#
    var credentials = new EdgeGridCredentials(
        host: "akab-h05tnam3wl42son7nktnlnnx-kbob3i3v.luna.akamaiapis.net",
        clientToken: "akab-c113ntt0k3n4qtari252bfxxbsl-yvsdj",
        clientSecret: "C113nt53KR3TN6N90yVuAgICxIRwsObLi0E67/N8eRN=",
        accessToken: "akab-acc35t0k3nodujqunph3w7hzp7-gtm6ij",
        headersToSign: new List<string> { "X-Custom-Header", "X-Another-Header" },
        maxBody: 131072
    );
    ```

## Sample application (`EdgeGridConsole.exe`)

The library offers a sample application that takes command-line parameters.

```shell
Usage: EdgeGridConeols <-e edgerc-file> <-s section> <-a account-switch-key>
           [-d data] [-f srcfile]
           [-o outfile]
           [-m max-size]
           [-X method]
           [-H header-line]
           [-T content-type]
           <url>

Where:
    -o outfile      Local file name to use to save the response from the API
    -d data         String of data to PUT to the API
    -f srcfile      Local file used as source when action=upload
    -H header-line  HTTP Header 'Name: value'
    -X method       Force HTTP PUT, POST, DELETE
    -T content-type The HTTP content type (default = application/json)
    url             Fully qualified API URL such as https://akab-1234.luna.akamaiapis.net/identity-management/v3/user-profile
```

Example:

```shell
EdgeGridConsole.exe -e ~/.edgerc -s default /identity-management/v3/user-profile
```

## Reporting issues

To report an issue or make a suggestion, create a new [GitHub issue](https://github.com/akamai/AkamaiOPEN-edgegrid-C-Sharp/issues).

## License

Copyright 2026 Akamai Technologies, Inc. All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.