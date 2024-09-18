# EdgeGrid for .NET/C#

This library implements an Authentication handler for the Akamai EdgeGrid Authentication scheme in .NET/C#.

You can find all Akamai APIs at https://techdocs.akamai.com/home/page/apis.

## Project organization

| Folder | Description|
| -------- | --------- |
| `/EdgeGridAuth` | The core auth signer project. |
| `/EdgeGridAuthTest` | The MSTest unit tests. |
| `/OpenAPI` | The generic `openapi.exe` tool to demonstrate using the signer. |
| `/Akamai.EdgeGrid.Auth.sln` | The root Visual Studio solution. |

## Install

1. Open `Akamai.EdgeGrid.Auth.sln` in Visual Studio and rebuild all or run `MSBuild.exe Akamai.EdgeGrid.Auth.sln /t:rebuild`.

2. Copy `Akamai.EdgeGrid.Auth.sln` to your application or solution.

## Authentication

We provide authentication credentials through an API client. Requests to the API are signed with a timestamp and are executed immediately.

1. [Create authentication credentials](https://techdocs.akamai.com/developer/docs/set-up-authentication-credentials).
   
2. Place your credentials in an EdgeGrid resource file, `.edgerc`, under a heading of `[default]` at your local home directory or the home directory of a web-server user.

    ```
    [default]
    client_secret = C113nt53KR3TN6N90yVuAgICxIRwsObLi0E67/N8eRN=
    host = akab-h05tnam3wl42son7nktnlnnx-kbob3i3v.luna.akamaiapis.net
    access_token = akab-acc35t0k3nodujqunph3w7hzp7-gtm6ij
    client_token = akab-c113ntt0k3n4qtari252bfxxbsl-yvsdj
    ```

## Use

To use the library, create an instance of the `EdgeGridV1Signer` and call either `Sign()` (if you're managing the HTTP communication yourself) or `Execute()` to use the built-in safety checks.

Provide details of your credentials from your local `.edgerc` resource file and pass them to the `ClientCredential()` method.

```csharp
using Akamai.EdgeGrid.Auth;

string clientToken = "akab-c113ntt0k3n4qtari252bfxxbsl-yvsdj";
string accessToken = "akab-acc35t0k3nodujqunph3w7hzp7-gtm6ij";
string secret = "C113nt53KR3TN6N90yVuAgICxIRwsObLi0E67/N8eRN=";

var signer = new EdgeGridV1Signer();
var credential = new ClientCredential(clientToken, accessToken, secret);

//TODO: create httpRequest via WebRequest.Create(uri);
signer.Sign(httpRequest, credential);
```

Alternatively, you can use the `Execute()` method to manage the connection and perform verification checks.

```csharp
using Akamai.EdgeGrid.Auth;

string clientToken = "akab-c113ntt0k3n4qtari252bfxxbsl-yvsdj";
string accessToken = "akab-acc35t0k3nodujqunph3w7hzp7-gtm6ij";
string secret = "C113nt53KR3TN6N90yVuAgICxIRwsObLi0E67/N8eRN=";

var signer = new EdgeGridV1Signer();
var credential = new ClientCredential(clientToken, accessToken, secret);

//TODO: create httpRequest via WebRequest.Create(uri);
signer.Execute(httpRequest, credential);
```

## Sample application (openapi.exe)

A sample application is available. It takes command line parameters.

```openapi.exe
-a akab-acc35t0k3nodujqunph3w7hzp7-gtm6ij
-c akab-c113ntt0k3n4qtari252bfxxbsl-yvsdj
-s C113nt53KR3TN6N90yVuAgICxIRwsObLi0E67/N8eRN=
https://akab-h05tnam3wl42son7nktnlnnx-kbob3i3v.luna.akamaiapis.net/identity-management/v3/user-profile
```

## Reporting issues

To report an issue or make a suggestion, create a new [GitHub issue](https://github.com/akamai/AkamaiOPEN-edgegrid-C-Sharp/issues).

## License

Copyright 2024 Akamai Technologies, Inc. All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use these files except in compliance with the License.
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.