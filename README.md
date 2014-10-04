# EdgeGridSigner (for .NET/c#)

This library assists in the interaction with Akamai's {Open} API found at http://developer.akamai.com. 
The API Spec can be found at: https://developer.akamai.com/api

## Project organization
* /EdgeGridAuth - core auth signere project
* /EdgeGridAuthTest - MSTest unit tests
* /OpenAPI - generic openapi.exe tool to demonstrate using the signer 
* /Akamai.EdgeGrid.Auth.sln - root VisualStudio solution

## Install
* Open the Akamai.EdgeGrid.Auth.sln in Visual Studio; Rebuild All
* OR ```MSBuild.exe Akamai.EdgeGrid.Auth.sln /t:rebuild```
* Copy the Akamai.EdgeGrid.Auth.dll to your application or solution. 

## Getting Started
* Create an instance of the `EdgeGridV1Signer` and call either Sign (if you are managing the http communication yourself
* or call Execute() to utilize built in safety checks

For example:
```
using Akamai.EdgeGrid.Auth;

var signer = new EdgeGridV1Signer();
var credential = new ClientCredential(clientToken, accessToken, secret);

//TODO: create httpRequest via WebRequest.Create(uri);
signer.Sign(httpRequest, credential);
```

alternatively, you can use the execute() method to manage the connection and perform verification checks
```
using Akamai.EdgeGrid.Auth;

var signer = new EdgeGridV1Signer();
var credential = new ClientCredential(clientToken, accessToken, secret); 

//TODO: create httpRequest via WebRequest.Create(uri);
signer.Execute(httpRequest, credential);
```


## Sample application (openapi.exe)
* A sample application has been created that can take command line parameters.

```openapi.exe
-a akab-access1234
-c akab-client1234 
-s secret1234
https://akab-url123.luna.akamaiapis.net/diagnostic-tools/v1/locations
```

