using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akamai.EdgeGrid.Auth;
using System;
using System.Net.Http;
using System.Text;

namespace Akamai.EdgeGrid.AuthTest
{
    [TestClass]
    public class EdgeGridV2SignerTest
    {
        private EdgeGridCredentials GetTestCredentials()
        {
            return new EdgeGridCredentials(
                host: "test.example.com",
                clientToken: "test-client-token",
                clientSecret: "test-secret",
                accessToken: "test-access-token"
            );
        }

        [TestMethod]
        public void TestSign_GET_Request()
        {
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://test.example.com/api/v1/test");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
            var authHeader = string.Join("", signedRequest.Headers.GetValues("Authorization"));
            Assert.IsTrue(authHeader.StartsWith("EG1-HMAC-SHA256"));
            Assert.IsTrue(authHeader.Contains("client_token=test-client-token"));
            Assert.IsTrue(authHeader.Contains("access_token=test-access-token"));
            Assert.IsTrue(authHeader.Contains("signature="));
        }

        [TestMethod]
        public void TestSign_POST_Request()
        {
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://test.example.com/api/v1/test");
            request.Content = new StringContent("{\"key\":\"value\"}", Encoding.UTF8, "application/json");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSign_NullRequestUri()
        {
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            var request = new HttpRequestMessage(HttpMethod.Get, (Uri)null);

            signer.Sign(request, credential);
        }

        [TestMethod]
        public void TestGetAuthHeader()
        {
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            byte[] requestBody = Encoding.UTF8.GetBytes("{\"test\":\"data\"}");

            string authHeader = signer.GetAuthHeader(credential, "POST", "/api/v1/test", requestBody);

            Assert.IsNotNull(authHeader);
            Assert.IsTrue(authHeader.StartsWith("EG1-HMAC-SHA256"));
            Assert.IsTrue(authHeader.Contains("client_token=test-client-token"));
            Assert.IsTrue(authHeader.Contains("access_token=test-access-token"));
            Assert.IsTrue(authHeader.Contains("timestamp="));
            Assert.IsTrue(authHeader.Contains("nonce="));
            Assert.IsTrue(authHeader.Contains("signature="));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetAuthHeader_NullClientSecret()
        {
            var signer = new EdgeGridV2Signer();
            // This should throw ArgumentNullException from the constructor
            var credential = new EdgeGridCredentials(
                host: "test.example.com",
                clientToken: "test-client-token",
                clientSecret: null,
                accessToken: "test-access-token"
            );
        }
    }
}
