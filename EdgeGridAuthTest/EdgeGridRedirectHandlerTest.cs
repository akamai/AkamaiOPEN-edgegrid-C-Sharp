#nullable enable
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akamai.EdgeGrid.Auth;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Akamai.EdgeGrid.AuthTest
{
    [TestClass]
    public class EdgeGridRedirectHandlerTest
    {
        private EdgeGridCredentials GetTestCredentials()
        {
            return new EdgeGridCredentials(
                host: "akaa-baseurl-xxxxxxxxxxx-xxxxxxxxxxxxx.luna.akamaiapis.net",
                clientToken: "akab-client-token-xxx-xxxxxxxxxxxxxxxx",
                clientSecret: "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=",
                accessToken: "akab-access-token-xxx-xxxxxxxxxxxxxxxx"
            );
        }

        [TestMethod]
        public async Task Test_RedirectHandler_FollowsRedirect()
        {
            // Create a mock handler that simulates a redirect
            var mockHandler = new MockRedirectHttpMessageHandler(
                initialResponse: new HttpResponseMessage(HttpStatusCode.Found)
                {
                    Headers = { Location = new Uri("https://example.com/redirected") }
                },
                redirectResponse: new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Success")
                }
            );

            var credentials = GetTestCredentials();
            var redirectHandler = new EdgeGridRedirectHandler(credentials)
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(redirectHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/original");
            
            // Sign the initial request
            var signer = new EdgeGridV2Signer();
            signer.Sign(request, credentials);

            var response = await client.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(2, mockHandler.RequestCount); // Original + redirect
        }

        [TestMethod]
        public async Task Test_RedirectHandler_ResignsRequest()
        {
            var mockHandler = new MockRedirectHttpMessageHandler(
                initialResponse: new HttpResponseMessage(HttpStatusCode.Found)
                {
                    Headers = { Location = new Uri("https://example.com/redirected") }
                },
                redirectResponse: new HttpResponseMessage(HttpStatusCode.OK)
            );

            var credentials = GetTestCredentials();
            var redirectHandler = new EdgeGridRedirectHandler(credentials)
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(redirectHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/original");
            
            var signer = new EdgeGridV2Signer();
            signer.Sign(request, credentials);

            await client.SendAsync(request);

            // Verify both requests had Authorization headers
            Assert.IsTrue(mockHandler.FirstRequestHadAuth);
            Assert.IsTrue(mockHandler.SecondRequestHadAuth);
        }

        [TestMethod]
        public async Task Test_RedirectHandler_MaxRedirectsExceeded()
        {
            // Create handler that always redirects
            var mockHandler = new MockInfiniteRedirectHandler();

            var credentials = GetTestCredentials();
            var redirectHandler = new EdgeGridRedirectHandler(credentials, maxRedirects: 3)
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(redirectHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/loop");
            
            var signer = new EdgeGridV2Signer();
            signer.Sign(request, credentials);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await client.SendAsync(request)
            );
        }

        [TestMethod]
        public async Task Test_RedirectHandler_NoRedirect()
        {
            var mockHandler = new MockRedirectHttpMessageHandler(
                initialResponse: new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("No redirect")
                },
                redirectResponse: null
            );

            var credentials = GetTestCredentials();
            var redirectHandler = new EdgeGridRedirectHandler(credentials)
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(redirectHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/no-redirect");
            
            var signer = new EdgeGridV2Signer();
            signer.Sign(request, credentials);

            var response = await client.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, mockHandler.RequestCount); // Only original request
        }

        // Mock HTTP handler for testing redirects
        private class MockRedirectHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _initialResponse;
            private readonly HttpResponseMessage? _redirectResponse;
            private int _requestCount = 0;

            public int RequestCount => _requestCount;
            public bool FirstRequestHadAuth { get; private set; }
            public bool SecondRequestHadAuth { get; private set; }

            public MockRedirectHttpMessageHandler(
                HttpResponseMessage initialResponse,
                HttpResponseMessage? redirectResponse)
            {
                _initialResponse = initialResponse;
                _redirectResponse = redirectResponse;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _requestCount++;

                var hasAuth = request.Headers.Contains("Authorization");
                if (_requestCount == 1)
                {
                    FirstRequestHadAuth = hasAuth;
                    return Task.FromResult(_initialResponse);
                }
                else
                {
                    SecondRequestHadAuth = hasAuth;
                    return Task.FromResult(_redirectResponse ?? new HttpResponseMessage(HttpStatusCode.OK));
                }
            }
        }

        // Mock handler that always redirects (for testing max redirects)
        private class MockInfiniteRedirectHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Found)
                {
                    Headers = { Location = new Uri("https://example.com/loop") }
                });
            }
        }
    }
}
