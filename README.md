[![Build Status](https://travis-ci.org/gfirem/AkamaiOPEN-edgegrid-C-Sharp.svg?branch=master)](https://travis-ci.org/gfirem/AkamaiOPEN-edgegrid-C-Sharp)

# EdgeGridSigner (for .NET/c#)

This library assists in the interaction with Akamai's {Open} API found at http://developer.akamai.com. 
The API Spec can be found at: https://developer.akamai.com/api

## Project organization
* /EdgeGrid-Signer-httpclient - core auth signer for .net httpclient
* /EdgeGridAuthTest - nunit unit tests
* /AkamaiEdgeGrid.sln - root VisualStudio solution
* /OpenApi - Cli example leveraging the EdgeGrid-Signer-httpclient library

## Install
* Open the Akamai.EdgeGrid.Auth.sln in Visual Studio; Rebuild All
* OR `dotnet build`
* Copy the AkamaiEdgeGrid.dll to your application or solution. 

## Getting Started

Create an instance of the `EdgeGridSigner` using any of the constructors available

```
using Akamai.EdgeGrid.Auth;

//Simple constructor
var signer = new EdgeGridSigner();

//Constructor with HttpMethod and Uri of the request
var signerWithMethodAndURI = new EdgeGridSigner(HttpMethod.Get, "https://test_host/diagnostic-tools/v2/ghost-locations/available");

//Constructor with HttpRequestMessage
HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://test_host/diagnostic-tools/v2/ghost-locations/available");
var signerWithHttpRequestMessage = new EdgeGridSigner(requestMessage);
```

Add the required Credentials to the Signer by either: 
* Creating your own credentials and passing it to the signer
```
    string client_token = "default_client_token";
    string access_token = "default_access_token";
    string client_secret = "default_client_secret";
    Credential credential = new Credential(client_token, access_token, client_secret);

    var signer = new EdgeGridSigner();
    signer.setCredential(credential);
```

* Loading the credentials from the environment
```
    var signer = new EdgeGridSigner();
    signer.getCredentialsFromEnvironment();
```

* Loading the credentials from an Edgerc file
```
    var signer = new EdgeGridSigner();
    signer.getCredentialsFromEdgerc("Section Name", "Path to the file");
```

The Signer constructor defaults to the 'GET' method if none is specified during the constructor, you can change this using setMethod
```
    var signer = new EdgeGridSigner();
    signer.setHttpMethod(HttpMethod.Get);
```

If the method is 'POST', a content-body must be set with setBodyContent or the signer will fail with an exception
```
    string postBody = "{\n    \"endUserName\": \"name\",\n    \"url\": \"www.test.com\"\n}";

    var signer = new EdgeGridSigner();
    signer.setHttpMethod(HttpMethod.Post);
    signer.setBodyContent(postBody);
```

An URL request is needed, add it if you didn't specify one in the constructor 
```
    signer.setRequestURI("https://default_host/diagnostic-tools/v2/end-users/diagnostic-url");
```

If all the requirements are meet then we can call GetRequestMessage which will generate our signed HttpRequestMessage
```
    HttpRequestMessage messageRequest = test.GetRequestMessage();
```

## Complete example of a get request

```
    string client_token = "Akamai_client_token";
    string access_token = "Akamai_access_token";
    string client_secret = "Akamai_client_secret";

    Credential credential = new Credential(client_token, access_token, client_secret);

    EdgeGridSigner signer = new EdgeGridSigner();
    signer.setCredential(credential);
    signer.setHttpMethod(HttpMethod.Get);
    signer.setRequestURI("https://default_host/diagnostic-tools/v2/ghost-locations/available");
    HttpRequestMessage messageRequest = signer.GetRequestMessage();

    HttpClient client = new HttpClient();
    var response = await client.SendAsync(requestMessage);

    response.Wait();
    //Process response after it finish

```

## Complete example of a post request

```
    string client_token = "Akamai_client_token";
    string access_token = "Akamai_access_token";
    string client_secret = "Akamai_client_secret";

    Credential credential = new Credential(client_token, access_token, client_secret);

    string postBody = "{\n    \"endUserName\": \"name\",\n    \"url\": \"www.test.com\"\n}";

    EdgeGridSigner signer = new EdgeGridSigner();
    signer.getCredentialsFromEdgerc("default", "../../../auth.edgerc");
    signer.setHttpMethod(HttpMethod.Post);
    signer.setBodyContent(postBody);
    signer.setRequestURI("https://default_host/diagnostic-tools/v2/end-users/diagnostic-url");
    HttpRequestMessage messageRequest = signer.GetRequestMessage();
    
    HttpClient client = new HttpClient();
    var response = await client.SendAsync(requestMessage);

    response.Wait();
    //Process response after it finish
```

## Notes:

* This signer was build specifically for the HttpClient, but if you want to use another client the signer has an option that generates the Authorization Header in string value.
   
```
    string client_token = "Akamai_client_token";
    string access_token = "Akamai_access_token";
    string client_secret = "Akamai_client_secret";

    Credential credential = new Credential(client_token, access_token, client_secret);

    string postBody = "{\n    \"endUserName\": \"name\",\n    \"url\": \"www.test.com\"\n}";

    EdgeGridSigner signer = new EdgeGridSigner();
    signer.getCredentialsFromEdgerc("default", "../../../auth.edgerc");
    signer.setHttpMethod(HttpMethod.Post);
    signer.setBodyContent(postBody);
    signer.setRequestURI("https://default_host/diagnostic-tools/v2/end-users/diagnostic-url");

    string AuthorizationString = signer.generateAuthorizationString();
    
```

* It should look like this:
```
client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=z1aM4VHWZURQ/iuN1t/OtqI1Y+612kmL3m4hTs49tYM=
```

## Sample application (OpenApi)
A sample application has been created that can take command line parameters. 
Check the instructions here: [OpenApi](/OpenApi)