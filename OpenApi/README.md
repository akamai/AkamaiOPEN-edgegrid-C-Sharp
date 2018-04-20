# OpenAPi CLI example using EdgeGridSigner 

This application is a CLI that leverages the EdgeGridSigner signer library.

## Getting Started

* Build the project in release mode.
* Copy executable and dll to a directory you want to store it

## How to use

* Get Request
```
    ./OpenApi --a "Access_Token" --c "Client_Token" --s  "Client_Secret" --d "{\n    \"endUserName\": \"name\",\n    \"url\": \"www.test.com\"\n}" "https://AkamaiHost.net/diagnostic-tools/v2/end-users/diagnostic-url"
```

* Post Request
```
    ./OpenApi   --a "Access_Token" --c "Client_Token" --s  "Client_Secret" --d "{    \"endUserName\": \"name\",    \"url\": \"www.test.com\"}" "https://AkamaiHost.net/diagnostic-tools/v2/end-users/diagnostic-url"
```

* Getting credentials from environment
```
    ./OpenApi  --e "true" "https://AkamaiHost.net/diagnostic-tools/v2/ghost-locations/available"
```

* Getting credentials from Edgerc file
```
    ./OpenApi  --r "PathToFile" "https://AkamaiHost.net/diagnostic-tools/v2/ghost-locations/available"
```

* Loading Post Body from file
```
    ./OpenApi --a "Access_Token" --c "Client_Token" --s "Client_Secret" --f "{workspace}/OpenApi/postBody" "https://AkamaiHost.net/diagnostic-tools/v2/end-users/diagnostic-url"
```