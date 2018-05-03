using System.Net.Http;
using Akamai.EdgeGrid;
using Akamai.EdgeGrid.Exception;
using NUnit.Framework;

namespace Tests
{
    public class EdgeGridSignerTest
    {

        [Test]
        public void TestEdgeGridConstructorDefaultCreation()
        {
            EdgeGridSigner Test = new EdgeGridSigner();
            Assert.NotNull(Test);
        }

        [Test]
        public void TestEdgeGridSecondConstructorCreation()
        {
            EdgeGridSigner Test = new EdgeGridSigner(HttpMethod.Get, "https://test_host/diagnostic-tools/v2/ghost-locations/available");
            Assert.NotNull(Test);
        }

        [Test]
        public void TestEdgeGridThirdConstructorCreation()
        {
            HttpRequestMessage RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://test_host/diagnostic-tools/v2/ghost-locations/available");
            EdgeGridSigner Test = new EdgeGridSigner(RequestMessage);
            Assert.NotNull(Test);
        }

        [Test]
        public void TestEdgeGridGenerateGetSignedRequest()
        {
            EdgeGridSigner Test = new EdgeGridSigner();
            Test.GetCredentialsFromEdgerc("default", "../../../auth.edgerc");
            Test.SetHttpMethod(HttpMethod.Get);
            Test.Timestamp = new EdgeGridTimestamp("20180416T20:52:22+0000");
            Test.Nonce = new EdgeGridNonce("ad5b09f0-2402-41df-9a35-a24ec46149b1");
            Test.SetRequestUri("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            HttpRequestMessage MessageRequest = Test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=ife78XmO9ajbUoZ6yKwgK0T44TywQxQeHkIUxY90oFo=";

            Assert.AreEqual(expectedAuthorizationscheme, MessageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, MessageRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public void TestEdgeGridGenerateGetAuthorizationString()
        {
            EdgeGridSigner Test = new EdgeGridSigner();
            Test.GetCredentialsFromEdgerc("default", "../../../auth.edgerc");
            Test.SetHttpMethod(HttpMethod.Get);
            Test.Timestamp = new EdgeGridTimestamp("20180416T20:52:22+0000");
            Test.Nonce = new EdgeGridNonce("ad5b09f0-2402-41df-9a35-a24ec46149b1");
            Test.SetRequestUri("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            string MessageRequest = Test.GenerateAuthorizationString();
            string expectedAuthorizationsValue = "EG1-HMAC-SHA256 client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=ife78XmO9ajbUoZ6yKwgK0T44TywQxQeHkIUxY90oFo=";

            Assert.AreEqual(expectedAuthorizationsValue, MessageRequest);
        }

        [Test]
        public void TestEdgeGridGeneratePostSignedRequest()
        {
            string postBody = "{\n    \"endUserName\": \"name\",\n    \"url\": \"www.test.com\"\n}";

            EdgeGridSigner Test = new EdgeGridSigner();
            Test.GetCredentialsFromEdgerc("default", "../../../auth.edgerc");
            Test.SetHttpMethod(HttpMethod.Post);
            Test.SetBodyContent(postBody);
            Test.Timestamp = new EdgeGridTimestamp("20180416T20:52:22+0000");
            Test.Nonce = new EdgeGridNonce("ad5b09f0-2402-41df-9a35-a24ec46149b1");
            Test.SetRequestUri("https://default_host/diagnostic-tools/v2/end-users/diagnostic-url");
            HttpRequestMessage MessageRequest = Test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=z1aM4VHWZURQ/iuN1t/OtqI1Y+612kmL3m4hTs49tYM=";

            Assert.AreEqual(expectedAuthorizationscheme, MessageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, MessageRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public void TestEdgeGridGenerateExtraHeadersSignedRequest()
        {
            string postBody = "{\n    \"endUserName\": \"name\",\n    \"url\": \"www.test.com\"\n}";

            EdgeGridSigner Test = new EdgeGridSigner();
            Test.GetCredentialsFromEdgerc("default", "../../../auth.edgerc");
            Test.SetHttpMethod(HttpMethod.Post);
            Test.SetBodyContent(postBody);
            Test.Timestamp = new EdgeGridTimestamp("20180416T20:52:22+0000");
            Test.Nonce = new EdgeGridNonce("ad5b09f0-2402-41df-9a35-a24ec46149b1");
            Test.AddApiCustomHeaders("x-a", "va");
            Test.AddApiCustomHeaders("x-c", "\"      xc        \"");
            Test.AddApiCustomHeaders("x-b", "    w         b");
            Test.SetRequestUri("https://default_host/diagnostic-tools/v2/end-users/diagnostic-url");
            HttpRequestMessage MessageRequest = Test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=U3v36bcZAwWqfl0CrdivBbxhjYdCOwDKYO8cKvEhuE8=";

            Assert.AreEqual(expectedAuthorizationscheme, MessageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, MessageRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public void TestEdgeGridGenerateCustomCredentialSignedRequest()
        {
            string clientToken = "default_client_token";
            string accessToken = "default_access_token";
            string clientSecret = "default_client_secret";

            Credential Credential = new Credential(clientToken, accessToken, clientSecret);

            EdgeGridSigner Test = new EdgeGridSigner();
            Test.SetCredential(Credential);
            Test.SetHttpMethod(HttpMethod.Get);
            Test.Timestamp = new EdgeGridTimestamp("20180416T20:52:22+0000");
            Test.Nonce = new EdgeGridNonce("ad5b09f0-2402-41df-9a35-a24ec46149b1");
            Test.SetRequestUri("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            HttpRequestMessage MessageRequest = Test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=ife78XmO9ajbUoZ6yKwgK0T44TywQxQeHkIUxY90oFo=";

            Assert.AreEqual(expectedAuthorizationscheme, MessageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, MessageRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public void TestEdgeGridGenerateSecondCustomCredentialSignedRequest()
        {
            string clientToken = "default_client_token";
            string host = "default_host";
            string accessToken = "default_access_token";
            string clientSecret = "default_client_secret";
            string maxBodySize = "2800";

            Credential Credential = new Credential(clientToken, accessToken, clientSecret, host, maxBodySize);

            EdgeGridSigner Test = new EdgeGridSigner();
            Test.SetCredential(Credential);
            Test.SetHttpMethod(HttpMethod.Get);
            Test.Timestamp = new EdgeGridTimestamp("20180416T20:52:22+0000");
            Test.Nonce = new EdgeGridNonce("ad5b09f0-2402-41df-9a35-a24ec46149b1");
            Test.SetRequestUri("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            HttpRequestMessage MessageRequest = Test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=ife78XmO9ajbUoZ6yKwgK0T44TywQxQeHkIUxY90oFo=";

            Assert.AreEqual(expectedAuthorizationscheme, MessageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, MessageRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public void TestEdgeGridHostDifferentFromUriException()
        {
            string clientToken = "default_client_token";
            string host = "otherhost";
            string accessToken = "default_access_token";
            string clientSecret = "default_client_secret";
            string maxBodySize = "2800";

            Credential Credential = new Credential(clientToken, accessToken, clientSecret, host, maxBodySize);

            EdgeGridSigner Test = new EdgeGridSigner();
            Test.SetCredential(Credential);
            Test.SetHttpMethod(HttpMethod.Get);
            Test.Timestamp = new EdgeGridTimestamp();
            Test.Nonce = new EdgeGridNonce();
            Test.SetRequestUri("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            Assert.Throws<EdgeGridSignerException>(() => Test.GetRequestMessage());
        }

        [Test]
        public void TestEdgeGridNoHostUriException()
        {
            EdgeGridSigner Test = new EdgeGridSigner();
            Test.SetHttpMethod(HttpMethod.Get);
            Assert.Throws<System.ArgumentNullException>(() => Test.GetRequestMessage());
        }

        [Test]
        public void TestEdgeGridNoClientTokenException()
        {
            Credential Credential = new Credential("", "test", "test", "test", "10");

            EdgeGridSigner Test = new EdgeGridSigner();
            Test.SetHttpMethod(HttpMethod.Get);
            Test.SetRequestUri("https://test/diagnostic-tools/v2/ghost-locations/available");
            Assert.Throws<System.ArgumentException>(() => Test.SetCredential(Credential));
        }

        [Test]
        public void TestEdgeGridNoAccessTokenException()
        {
            Credential Credential = new Credential("test", "", "test", "test", "10");

            EdgeGridSigner Test = new EdgeGridSigner();
            Test.SetHttpMethod(HttpMethod.Get);
            Test.SetRequestUri("https://test/diagnostic-tools/v2/ghost-locations/available");
            Assert.Throws<System.ArgumentException>(() => Test.SetCredential(Credential));
        }

        [Test]
        public void TestEdgeGridNoClientSecretException()
        {
            Credential Credential = new Credential("test", "test", "", "test", "10");

            EdgeGridSigner Test = new EdgeGridSigner();
            Test.SetHttpMethod(HttpMethod.Get);
            Test.SetRequestUri("https://test/diagnostic-tools/v2/ghost-locations/available");
            Assert.Throws<System.ArgumentException>(() => Test.SetCredential(Credential));
        }

        [Test]
        public void TestEdgeGridNoPostContentException()
        {
            EdgeGridSigner Test = new EdgeGridSigner();
            Test.GetCredentialsFromEdgerc("default", "../../../auth.edgerc");
            Test.SetHttpMethod(HttpMethod.Post);
            Test.Timestamp = new EdgeGridTimestamp();
            Test.Nonce = new EdgeGridNonce();
            Test.SetRequestUri("https://default_host/diagnostic-tools/v2/end-users/diagnostic-url");
            Assert.Throws<EdgeGridSignerException>(() => Test.GetRequestMessage());
        }
    }
}