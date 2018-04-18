using NUnit.Framework;
using Akamai.EdgeGrid;
using System.Net.Http;

namespace Tests
{
    public class EdgeGridSignerTest
    {

        [Test]
        public void Test_ConstructorDefaultCreation()
        {
            EdgeGridSigner test = new EdgeGridSigner();
            Assert.NotNull(test);
        }
        [Test]
        public void Test_SecondConstructorCreation()
        {
            EdgeGridSigner test = new EdgeGridSigner(HttpMethod.Get, "https://test_host/diagnostic-tools/v2/ghost-locations/available");
            Assert.NotNull(test);
        }

        [Test]
        public void Test_ThirdConstructorCreation()
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://test_host/diagnostic-tools/v2/ghost-locations/available");
            EdgeGridSigner test = new EdgeGridSigner(requestMessage);
            Assert.NotNull(test);
        }


        [Test]
        public void Test_generateGetSignedRequest()
        {
            EdgeGridSigner test = new EdgeGridSigner();
            test.getCredentialsFromEdgerc("default", "../../../auth.edgerc");
            test.setHttpMethod(HttpMethod.Get);
            test.Timestamp  =  "20180416T20:52:22+0000";
            test.Nonce = "ad5b09f0-2402-41df-9a35-a24ec46149b1";
            test.setRequestURI("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            HttpRequestMessage messageRequest = test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=ife78XmO9ajbUoZ6yKwgK0T44TywQxQeHkIUxY90oFo=";

            Assert.AreEqual(expectedAuthorizationscheme ,messageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, messageRequest.Headers.Authorization.Parameter);
        }
        
        [Test]
        public void Test_generateGetAuthorizationString()
        {
            EdgeGridSigner test = new EdgeGridSigner();
            test.getCredentialsFromEdgerc("default", "../../../auth.edgerc");
            test.setHttpMethod(HttpMethod.Get);
            test.Timestamp  =  "20180416T20:52:22+0000";
            test.Nonce = "ad5b09f0-2402-41df-9a35-a24ec46149b1";
            test.setRequestURI("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            string messageRequest = test.generateAuthorizationString();
            string expectedAuthorizationsValue = "EG1-HMAC-SHA256 client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=ife78XmO9ajbUoZ6yKwgK0T44TywQxQeHkIUxY90oFo=";

            Assert.AreEqual(expectedAuthorizationsValue, messageRequest);
        }

        [Test]
        public void Test_generatePostSignedRequest()
        {
            string postBody = "{\n    \"endUserName\": \"name\",\n    \"url\": \"www.test.com\"\n}";

            EdgeGridSigner test = new EdgeGridSigner();
            test.getCredentialsFromEdgerc("default", "../../../auth.edgerc");
            test.setHttpMethod(HttpMethod.Post);
            test.setBodyContent(postBody);
            test.Timestamp  =  "20180416T20:52:22+0000";
            test.Nonce = "ad5b09f0-2402-41df-9a35-a24ec46149b1";
            test.setRequestURI("https://default_host/diagnostic-tools/v2/end-users/diagnostic-url");
            HttpRequestMessage messageRequest = test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=z1aM4VHWZURQ/iuN1t/OtqI1Y+612kmL3m4hTs49tYM=";

            Assert.AreEqual(expectedAuthorizationscheme ,messageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, messageRequest.Headers.Authorization.Parameter);
        }


        [Test]
        public void Test_generateExtraHeadersSignedRequest()
        {
            string postBody = "{\n    \"endUserName\": \"name\",\n    \"url\": \"www.test.com\"\n}";

            EdgeGridSigner test = new EdgeGridSigner();
            test.getCredentialsFromEdgerc("default", "../../../auth.edgerc");
            test.setHttpMethod(HttpMethod.Post);
            test.setBodyContent(postBody);
            test.Timestamp  =  "20180416T20:52:22+0000";
            test.Nonce = "ad5b09f0-2402-41df-9a35-a24ec46149b1";
            test.addApiCustomHeaders("x-a","va" );
            test.addApiCustomHeaders("x-c","\"      xc        \"");
            test.addApiCustomHeaders("x-b","    w         b");
            test.setRequestURI("https://default_host/diagnostic-tools/v2/end-users/diagnostic-url");
            HttpRequestMessage messageRequest = test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=U3v36bcZAwWqfl0CrdivBbxhjYdCOwDKYO8cKvEhuE8=";

            Assert.AreEqual(expectedAuthorizationscheme ,messageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, messageRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public void Test_generateCustomCredentialSignedRequest()
        {
            string client_token = "default_client_token";
            string access_token = "default_access_token";
            string client_secret = "default_client_secret";

            Credential credential = new Credential(client_token, access_token, client_secret);

            EdgeGridSigner test = new EdgeGridSigner();
            test.setCredential(credential);
            test.setHttpMethod(HttpMethod.Get);
            test.Timestamp  =  "20180416T20:52:22+0000";
            test.Nonce = "ad5b09f0-2402-41df-9a35-a24ec46149b1";
            test.setRequestURI("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            HttpRequestMessage messageRequest = test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=ife78XmO9ajbUoZ6yKwgK0T44TywQxQeHkIUxY90oFo=";

            Assert.AreEqual(expectedAuthorizationscheme ,messageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, messageRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public void Test_generateSecondCustomCredentialSignedRequest()
        {
            string client_token = "default_client_token";
            string host = "default_host";
            string access_token = "default_access_token";
            string client_secret = "default_client_secret";
            string max_body_size = "2800";

            Credential credential = new Credential(client_token, access_token, client_secret, host, max_body_size);

            EdgeGridSigner test = new EdgeGridSigner();
            test.setCredential(credential);
            test.setHttpMethod(HttpMethod.Get);
            test.Timestamp  =  "20180416T20:52:22+0000";
            test.Nonce = "ad5b09f0-2402-41df-9a35-a24ec46149b1";
            test.setRequestURI("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            HttpRequestMessage messageRequest = test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=ife78XmO9ajbUoZ6yKwgK0T44TywQxQeHkIUxY90oFo=";

            Assert.AreEqual(expectedAuthorizationscheme ,messageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, messageRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public void Test_HostDifferentFromURIException()
        {
            string client_token = "default_client_token";
            string host = "otherhost";
            string access_token = "default_access_token";
            string client_secret = "default_client_secret";
            string max_body_size = "2800";

            Credential credential = new Credential(client_token, access_token, client_secret, host, max_body_size);

            EdgeGridSigner test = new EdgeGridSigner();
            test.setCredential(credential);
            test.setHttpMethod(HttpMethod.Get);
            test.Timestamp  =  "20180416T20:52:22+0000";
            test.Nonce = "ad5b09f0-2402-41df-9a35-a24ec46149b1";
            test.setRequestURI("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            string exceptionMessage ="";
            try {
                HttpRequestMessage messageRequest = test.GetRequestMessage();
            }catch(System.Exception exception){
                exceptionMessage = exception.Message;
            }
            string expectedExceptionMessage= "URI Request does not match the HOST from the loaded Credentials";

            Assert.AreEqual(expectedExceptionMessage ,exceptionMessage);
        }

        [Test]
        public void Test_HostDifferentNoURIException()
        {
            EdgeGridSigner test = new EdgeGridSigner();
            test.setHttpMethod(HttpMethod.Get);
            string exceptionMessage ="";
            try {
            HttpRequestMessage messageRequest = test.GetRequestMessage();
            }catch(System.Exception exception){
                exceptionMessage = exception.Message;
            }
            string expectedExceptionMessage= "Request URL not Set";


            Assert.AreEqual(expectedExceptionMessage ,exceptionMessage);
        }

        [Test]
        public void Test_NoClientTokenException()
        {
            string client_token = "";
            Credential credential = new Credential(client_token, "test", "test", "test", "10");

            EdgeGridSigner test = new EdgeGridSigner();
            test.setHttpMethod(HttpMethod.Get);
            test.setRequestURI("https://test/diagnostic-tools/v2/ghost-locations/available");
            test.setCredential(credential);
            string exceptionMessage ="";
            try {
            HttpRequestMessage messageRequest = test.GetRequestMessage();
            }catch(System.Exception exception){
                exceptionMessage = exception.Message;
            }
            string expectedExceptionMessage= "ClientToken is null or has not been loaded";


            Assert.AreEqual(expectedExceptionMessage ,exceptionMessage);
        }

        [Test]
        public void Test_NoAccessTokenException()
        {
            string access_token = "";
            Credential credential = new Credential("test", access_token,"test", "test", "10");

            EdgeGridSigner test = new EdgeGridSigner();
            test.setHttpMethod(HttpMethod.Get);
            test.setRequestURI("https://test/diagnostic-tools/v2/ghost-locations/available");
            test.setCredential(credential);
            string exceptionMessage ="";
            try {
            HttpRequestMessage messageRequest = test.GetRequestMessage();
            }catch(System.Exception exception){
                exceptionMessage = exception.Message;
            }
            string expectedExceptionMessage= "AccessToken is null or has not been loaded";


            Assert.AreEqual(expectedExceptionMessage ,exceptionMessage);
        }

        [Test]
        public void Test_NoClientSecretException()
        {
            string Client_Secret = "";
            Credential credential = new Credential("test", "test",Client_Secret, "test", "10");

            EdgeGridSigner test = new EdgeGridSigner();
            test.setHttpMethod(HttpMethod.Get);
            test.setRequestURI("https://test/diagnostic-tools/v2/ghost-locations/available");
            test.setCredential(credential);
            string exceptionMessage ="";
            try {
            HttpRequestMessage messageRequest = test.GetRequestMessage();
            }catch(System.Exception exception){
                exceptionMessage = exception.Message;
            }
            string expectedExceptionMessage= "ClientSecret is null or has not been loaded";


            Assert.AreEqual(expectedExceptionMessage ,exceptionMessage);
        }

        [Test]
        public void Test_NoPostContentException()
        {
            EdgeGridSigner test = new EdgeGridSigner();
            test.getCredentialsFromEdgerc("default", "../../../auth.edgerc");
            test.setHttpMethod(HttpMethod.Post);
            test.Timestamp  =  "20180416T20:52:22+0000";
            test.Nonce = "ad5b09f0-2402-41df-9a35-a24ec46149b1";
            test.setRequestURI("https://default_host/diagnostic-tools/v2/end-users/diagnostic-url");
            string exceptionMessage ="";
            try {
            HttpRequestMessage messageRequest = test.GetRequestMessage();
            }catch(System.Exception exception){
                exceptionMessage = exception.Message;
            }
            string expectedExceptionMessage= "Body content not set for this message request";


            Assert.AreEqual(expectedExceptionMessage ,exceptionMessage);
        }

    }
}