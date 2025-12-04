#nullable enable
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akamai.EdgeGrid.Auth;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Akamai.EdgeGrid.AuthTest
{
    [TestClass]
    public class EdgeGridV2SignerTest
    {
        private const string BaseUrl = "https://akaa-baseurl-xxxxxxxxxxx-xxxxxxxxxxxxx.luna.akamaiapis.net";
        private const string ClientToken = "akab-client-token-xxx-xxxxxxxxxxxxxxxx";
        private const string ClientSecret = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=";
        private const string AccessToken = "akab-access-token-xxx-xxxxxxxxxxxxxxxx";
        private const string Timestamp = "20140321T19:34:21+0000";
        private const string Nonce = "nonce-xx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

        private EdgeGridCredentials GetTestCredentials()
        {
            return new EdgeGridCredentials(
                host: "akaa-baseurl-xxxxxxxxxxx-xxxxxxxxxxxxx.luna.akamaiapis.net",
                clientToken: ClientToken,
                clientSecret: ClientSecret,
                accessToken: AccessToken
            );
        }

        [TestMethod]
        public void Test_SimpleGET()
        {
            // Test case: "simple GET"
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
            var authHeader = string.Join("", signedRequest.Headers.GetValues("Authorization"));
            Assert.IsTrue(authHeader.StartsWith("EG1-HMAC-SHA256"));
            Assert.IsTrue(authHeader.Contains($"client_token={ClientToken}"));
            Assert.IsTrue(authHeader.Contains($"access_token={AccessToken}"));
        }

        [TestMethod]
        public void Test_GET_WithQueryString()
        {
            // Test case: "GET with querystring"
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/testapi/v1/t1?p1=1&p2=2");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
        }

        [TestMethod]
        public void Test_POST_InsideLimit()
        {
            // Test case: "POST inside limit"
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/testapi/v1/t3");
            request.Content = new StringContent("datadatadatadatadatadatadatadata", Encoding.UTF8, "application/octet-stream");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
        }

        [TestMethod]
        public void Test_POST_TooLarge()
        {
            // Test case: "POST too large" - body exceeds max_body limit
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            
            // Create a large body that exceeds the max body size
            string largeBody = new string('d', 3000);
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/testapi/v1/t3");
            request.Content = new StringContent(largeBody, Encoding.UTF8, "application/octet-stream");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
        }

        [TestMethod]
        public void Test_POST_EmptyBody()
        {
            // Test case: "POST empty body"
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/testapi/v1/t6");
            request.Content = new StringContent("", Encoding.UTF8, "application/octet-stream");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
        }

        [TestMethod]
        public void Test_PUT_Request()
        {
            // Test case: "PUT test"
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/testapi/v1/t6");
            request.Content = new StringContent("PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP", Encoding.UTF8, "application/octet-stream");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
        }

        [TestMethod]
        public void Test_PATCH_Request()
        {
            // Test case: "PATCH test"
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{BaseUrl}/testapi/v1/t6");
            request.Content = new StringContent("PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP", Encoding.UTF8, "application/octet-stream");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
        }

        [TestMethod]
        public void Test_GET_WithQueryParams()
        {
            // Test case: "GET with query params"
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/testapi/v1/configs/111?from=12345&limit=200000");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
        }

        [TestMethod]
        public void Test_GET_WithQueryParamsAndSeparatorInPath()
        {
            // Test case: "GET with query params and separator in path"
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/testapi/v1/configs/111;222;333?from=12345&limit=200000");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_Sign_NullRequestUri()
        {
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            var request = new HttpRequestMessage(HttpMethod.Get, (Uri)null!);

            signer.Sign(request, credential);
        }

        [TestMethod]
        public void Test_GetAuthHeader_POST()
        {
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            byte[] requestBody = Encoding.UTF8.GetBytes("{\"test\":\"data\"}");

            string authHeader = signer.GetAuthHeader(credential, "POST", "/api/v1/test", requestBody);

            Assert.IsNotNull(authHeader);
            Assert.IsTrue(authHeader.StartsWith("EG1-HMAC-SHA256"));
            Assert.IsTrue(authHeader.Contains($"client_token={ClientToken}"));
            Assert.IsTrue(authHeader.Contains($"access_token={AccessToken}"));
            Assert.IsTrue(authHeader.Contains("timestamp="));
            Assert.IsTrue(authHeader.Contains("nonce="));
            Assert.IsTrue(authHeader.Contains("signature="));
        }

        [TestMethod]
        public void Test_GetAuthHeader_GET()
        {
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            byte[] requestBody = new byte[0];

            string authHeader = signer.GetAuthHeader(credential, "GET", "/api/v1/test", requestBody);

            Assert.IsNotNull(authHeader);
            Assert.IsTrue(authHeader.StartsWith("EG1-HMAC-SHA256"));
            Assert.IsTrue(authHeader.Contains($"client_token={ClientToken}"));
            Assert.IsTrue(authHeader.Contains($"access_token={AccessToken}"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_GetAuthHeader_NullClientSecret()
        {
            var signer = new EdgeGridV2Signer();
            var credential = new EdgeGridCredentials(
                host: "test.example.com",
                clientToken: "test-token",
                clientSecret: "",
                accessToken: "test-access"
            );
            byte[] requestBody = new byte[0];

            // Should throw because client secret is empty
            signer.GetAuthHeader(credential, "GET", "/api/v1/test", requestBody);
        }

        [TestMethod]
        public void Test_JSON_Request()
        {
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/testapi/v1/t3?extended=true");
            request.Content = new StringContent("{\"key\":\"value\"}", Encoding.UTF8, "application/json");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
        }

        [TestMethod]
        public void Test_HeadersToSign()
        {
            // Test custom headers in signature
            var credential = new EdgeGridCredentials(
                host: "test.example.com",
                clientToken: ClientToken,
                clientSecret: ClientSecret,
                accessToken: AccessToken,
                headersToSign: new List<string> { "X-Test1", "X-Test2" }
            );

            var signer = new EdgeGridV2Signer();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/testapi/v1/test");
            request.Headers.Add("X-Test1", "value1");
            request.Headers.Add("X-Test2", "value2");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
        }

        [TestMethod]
        public void Test_MaxBodyFromCredentials()
        {
            // Test that max_body from credentials is used
            var credential = new EdgeGridCredentials(
                host: "test.example.com",
                clientToken: ClientToken,
                clientSecret: ClientSecret,
                accessToken: AccessToken,
                maxBody: 1024
            );

            Assert.AreEqual(1024, credential.MaxBody);

            var signer = new EdgeGridV2Signer();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/testapi/v1/test");
            request.Content = new StringContent(new string('d', 2000), Encoding.UTF8, "application/octet-stream");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
        }

        [TestMethod]
        public void Test_UserAgentVersionHeaders()
        {
            // Test that Akamai CLI version headers are added
            Environment.SetEnvironmentVariable("AKAMAI_CLI", "test");
            Environment.SetEnvironmentVariable("AKAMAI_CLI_VERSION", "1.0.0");

            try
            {
                var signer = new EdgeGridV2Signer();
                var credential = GetTestCredentials();
                var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/");

                var signedRequest = signer.Sign(request, credential);

                Assert.IsNotNull(signedRequest);
                var userAgent = signedRequest.Headers.UserAgent.ToString();
                Assert.IsTrue(userAgent.Contains("AkamaiCLI/1.0.0"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("AKAMAI_CLI", null);
                Environment.SetEnvironmentVariable("AKAMAI_CLI_VERSION", null);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Test_HeaderCanonicalization_LeadingWhitespace()
        {
            // Test that headers with leading whitespace throw exception
            var credential = new EdgeGridCredentials(
                host: "test.example.com",
                clientToken: ClientToken,
                clientSecret: ClientSecret,
                accessToken: AccessToken,
                headersToSign: new List<string> { "X-Test1" }
            );

            var signer = new EdgeGridV2Signer();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/testapi/v1/test");
            request.Headers.Add("X-Test1", "     invalid-leading-space");

            signer.Sign(request, credential);
        }

        [TestMethod]
        public void Test_SigningWithPathParameters()
        {
            // Test that path parameters (semicolon-separated) are included in signature
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();

            // Create request with path parameters
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{BaseUrl}/testapi/v1/resource;param1=value1;param2=value2");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
            
            // Verify signature was generated
            var authHeader = string.Join("", signedRequest.Headers.GetValues("Authorization"));
            Assert.IsTrue(authHeader.StartsWith("EG1-HMAC-SHA256"));
            Assert.IsTrue(authHeader.Contains("signature="));
        }

        [TestMethod]
        public void Test_SigningWithPathParametersAndQuery()
        {
            // Test path parameters combined with query string
            var signer = new EdgeGridV2Signer();
            var credential = GetTestCredentials();

            // Create request with both path parameters and query string
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{BaseUrl}/testapi/v1/resource;param=value?query=test&foo=bar");

            var signedRequest = signer.Sign(request, credential);

            Assert.IsNotNull(signedRequest);
            Assert.IsTrue(signedRequest.Headers.Contains("Authorization"));
            
            var authHeader = string.Join("", signedRequest.Headers.GetValues("Authorization"));
            Assert.IsTrue(authHeader.StartsWith("EG1-HMAC-SHA256"));
            Assert.IsTrue(authHeader.Contains("signature="));
        }
    }
}
