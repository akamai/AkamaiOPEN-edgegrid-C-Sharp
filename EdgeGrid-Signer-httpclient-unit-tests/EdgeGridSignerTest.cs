using System.Net.Http;
using Akamai.EdgeGrid;
using AkamaiEdgeGrid.EdgeGrid;
using Akamai.EdgeGrid.Exception;
using NUnit.Framework;

namespace Tests
{
    public class EdgeGridSignerTest
    {

        [Test]
        public void TestEdgeGridConstructorDefaultCreation()
        {
            EdgeGridSigner test = new EdgeGridSigner();
            Assert.NotNull(test);
        }

        [Test]
        public void TestEdgeGridSecondConstructorCreation()
        {
            EdgeGridSigner test = new EdgeGridSigner(HttpMethod.Get, "https://test_host/diagnostic-tools/v2/ghost-locations/available");
            Assert.NotNull(test);
        }

        [Test]
        public void TestEdgeGridThirdConstructorCreation()
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://test_host/diagnostic-tools/v2/ghost-locations/available");
            EdgeGridSigner test = new EdgeGridSigner(requestMessage);
            Assert.NotNull(test);
        }

        [Test]
        public void TestEdgeGridGenerateGetSignedRequest()
        {
            EdgeGridSigner test = new EdgeGridSigner();
            test.GetCredentialsFromEdgerc("default", "../../../auth.edgerc");
            test.SetHttpMethod(HttpMethod.Get);
            test.Timestamp = new EdgeGridTimestamp("20180416T20:52:22+0000");
            test.Nonce = new EdgeGridNonce("ad5b09f0-2402-41df-9a35-a24ec46149b1");
            test.SetRequestURI("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            HttpRequestMessage messageRequest = test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=ife78XmO9ajbUoZ6yKwgK0T44TywQxQeHkIUxY90oFo=";

            Assert.AreEqual(expectedAuthorizationscheme, messageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, messageRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public void TestEdgeGridGenerateGetAuthorizationString()
        {
            EdgeGridSigner test = new EdgeGridSigner();
            test.GetCredentialsFromEdgerc("default", "../../../auth.edgerc");
            test.SetHttpMethod(HttpMethod.Get);
            test.Timestamp = new EdgeGridTimestamp("20180416T20:52:22+0000");
            test.Nonce = new EdgeGridNonce("ad5b09f0-2402-41df-9a35-a24ec46149b1");
            test.SetRequestURI("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            string messageRequest = test.GenerateAuthorizationString();
            string expectedAuthorizationsValue = "EG1-HMAC-SHA256 client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=ife78XmO9ajbUoZ6yKwgK0T44TywQxQeHkIUxY90oFo=";

            Assert.AreEqual(expectedAuthorizationsValue, messageRequest);
        }

        [Test]
        public void TestEdgeGridGeneratePostSignedRequest()
        {
            string postBody = "{\n    \"endUserName\": \"name\",\n    \"url\": \"www.test.com\"\n}";

            EdgeGridSigner test = new EdgeGridSigner();
            test.GetCredentialsFromEdgerc("default", "../../../auth.edgerc");
            test.SetHttpMethod(HttpMethod.Post);
            test.SetBodyContent(postBody);
            test.Timestamp = new EdgeGridTimestamp("20180416T20:52:22+0000");
            test.Nonce = new EdgeGridNonce("ad5b09f0-2402-41df-9a35-a24ec46149b1");
            test.SetRequestURI("https://default_host/diagnostic-tools/v2/end-users/diagnostic-url");
            HttpRequestMessage messageRequest = test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=z1aM4VHWZURQ/iuN1t/OtqI1Y+612kmL3m4hTs49tYM=";

            Assert.AreEqual(expectedAuthorizationscheme, messageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, messageRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public void TestEdgeGridGenerateExtraHeadersSignedRequest()
        {
            string postBody = "{\n    \"endUserName\": \"name\",\n    \"url\": \"www.test.com\"\n}";

            EdgeGridSigner test = new EdgeGridSigner();
            test.GetCredentialsFromEdgerc("default", "../../../auth.edgerc");
            test.SetHttpMethod(HttpMethod.Post);
            test.SetBodyContent(postBody);
            test.Timestamp = new EdgeGridTimestamp("20180416T20:52:22+0000");
            test.Nonce = new EdgeGridNonce("ad5b09f0-2402-41df-9a35-a24ec46149b1");
            test.AddApiCustomHeaders("x-a", "va");
            test.AddApiCustomHeaders("x-c", "\"      xc        \"");
            test.AddApiCustomHeaders("x-b", "    w         b");
            test.SetRequestURI("https://default_host/diagnostic-tools/v2/end-users/diagnostic-url");
            HttpRequestMessage messageRequest = test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=U3v36bcZAwWqfl0CrdivBbxhjYdCOwDKYO8cKvEhuE8=";

            Assert.AreEqual(expectedAuthorizationscheme, messageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, messageRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public void TestEdgeGridGenerateCustomCredentialSignedRequest()
        {
            string clientToken = "default_client_token";
            string accessToken = "default_access_token";
            string clientSecret = "default_client_secret";

            Credential credential = new Credential(clientToken, accessToken, clientSecret);

            EdgeGridSigner test = new EdgeGridSigner();
            test.SetCredential(credential);
            test.SetHttpMethod(HttpMethod.Get);
            test.Timestamp = new EdgeGridTimestamp("20180416T20:52:22+0000");
            test.Nonce = new EdgeGridNonce("ad5b09f0-2402-41df-9a35-a24ec46149b1");
            test.SetRequestURI("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            HttpRequestMessage messageRequest = test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=ife78XmO9ajbUoZ6yKwgK0T44TywQxQeHkIUxY90oFo=";

            Assert.AreEqual(expectedAuthorizationscheme, messageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, messageRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public void TestEdgeGridGenerateSecondCustomCredentialSignedRequest()
        {
            string clientToken = "default_client_token";
            string host = "default_host";
            string accessToken = "default_access_token";
            string clientSecret = "default_client_secret";
            string maxBodySize = "2800";

            Credential credential = new Credential(clientToken, accessToken, clientSecret, host, maxBodySize);

            EdgeGridSigner test = new EdgeGridSigner();
            test.SetCredential(credential);
            test.SetHttpMethod(HttpMethod.Get);
            test.Timestamp = new EdgeGridTimestamp("20180416T20:52:22+0000");
            test.Nonce = new EdgeGridNonce("ad5b09f0-2402-41df-9a35-a24ec46149b1");
            test.SetRequestURI("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            HttpRequestMessage messageRequest = test.GetRequestMessage();

            string expectedAuthorizationscheme = "EG1-HMAC-SHA256";
            string expectedAuthorizationsValue = "client_token=default_client_token;access_token=default_access_token;timestamp=20180416T20:52:22+0000;nonce=ad5b09f0-2402-41df-9a35-a24ec46149b1;signature=ife78XmO9ajbUoZ6yKwgK0T44TywQxQeHkIUxY90oFo=";

            Assert.AreEqual(expectedAuthorizationscheme, messageRequest.Headers.Authorization.Scheme);
            Assert.AreEqual(expectedAuthorizationsValue, messageRequest.Headers.Authorization.Parameter);
        }

        [Test]
        public void TestEdgeGridHostDifferentFromURIException()
        {
            string clientToken = "default_client_token";
            string host = "otherhost";
            string accessToken = "default_access_token";
            string clientSecret = "default_client_secret";
            string maxBodySize = "2800";

            Credential credential = new Credential(clientToken, accessToken, clientSecret, host, maxBodySize);

            EdgeGridSigner test = new EdgeGridSigner();
            test.SetCredential(credential);
            test.SetHttpMethod(HttpMethod.Get);
            test.Timestamp = new EdgeGridTimestamp();
            test.Nonce = new EdgeGridNonce();
            test.SetRequestURI("https://default_host/diagnostic-tools/v2/ghost-locations/available");
            Assert.Throws<EdgeGridSignerException>(() => test.GetRequestMessage());
        }

        [Test]
        public void TestEdgeGridNoHostUriException()
        {
            EdgeGridSigner test = new EdgeGridSigner();
            test.SetHttpMethod(HttpMethod.Get);
            Assert.Throws<System.ArgumentNullException>(() => test.GetRequestMessage());
        }

        [Test]
        public void TestEdgeGridNoClientTokenException()
        {
            Credential credential = new Credential("", "test", "test", "test", "10");

            EdgeGridSigner test = new EdgeGridSigner();
            test.SetHttpMethod(HttpMethod.Get);
            test.SetRequestURI("https://test/diagnostic-tools/v2/ghost-locations/available");
            Assert.Throws<System.ArgumentException>(() => test.SetCredential(credential));
        }

        [Test]
        public void TestEdgeGridNoAccessTokenException()
        {
            Credential credential = new Credential("test", "", "test", "test", "10");

            EdgeGridSigner test = new EdgeGridSigner();
            test.SetHttpMethod(HttpMethod.Get);
            test.SetRequestURI("https://test/diagnostic-tools/v2/ghost-locations/available");
            Assert.Throws<System.ArgumentException>(() => test.SetCredential(credential));
        }

        [Test]
        public void TestEdgeGridNoClientSecretException()
        {
            Credential credential = new Credential("test", "test", "", "test", "10");

            EdgeGridSigner test = new EdgeGridSigner();
            test.SetHttpMethod(HttpMethod.Get);
            test.SetRequestURI("https://test/diagnostic-tools/v2/ghost-locations/available");
            Assert.Throws<System.ArgumentException>(() => test.SetCredential(credential));
        }

        [Test]
        public void TestEdgeGridNoPostContentException()
        {
            EdgeGridSigner test = new EdgeGridSigner();
            test.GetCredentialsFromEdgerc("default", "../../../auth.edgerc");
            test.SetHttpMethod(HttpMethod.Post);
            test.Timestamp = new EdgeGridTimestamp();
            test.Nonce = new EdgeGridNonce();
            test.SetRequestURI("https://default_host/diagnostic-tools/v2/end-users/diagnostic-url");
            Assert.Throws<EdgeGridSignerException>(() => test.GetRequestMessage());
        }
    }
}