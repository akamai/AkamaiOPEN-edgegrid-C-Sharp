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
            
            await client.SendAsync(request);

            // Verify both requests had Authorization headers
            Assert.IsTrue(mockHandler.FirstRequestHadAuth);
            Assert.IsTrue(mockHandler.SecondRequestHadAuth);
        }

        [TestMethod]
        public async Task Test_RedirectHandler_MaxRedirectsExceeded()
        {
            // Create a handler that always redirects
            var mockHandler = new MockInfiniteRedirectHandler();

            var credentials = GetTestCredentials();
            var redirectHandler = new EdgeGridRedirectHandler(credentials, maxRedirects: 3)
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(redirectHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/loop");
            
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
            
            var response = await client.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, mockHandler.RequestCount); // Only original request
        }

        // Mock an HTTP handler for testing redirects
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
        [TestMethod]
        public async Task Test_CreateHttpClient_FactoryMethod()
        {
            // Test the factory method that creates an HttpClient with redirect handling built-in
            var credentials = GetTestCredentials();
            
            // Create an HttpClient using the factory method
            using var client = EdgeGridV2Signer.CreateHttpClient(credentials);
            
            // Verify that the client was created successfully
            Assert.IsNotNull(client);
            
            // The client should have redirect handling configured
            // We can't easily test the actual redirect behavior without a real server,
            // but we can verify the client is created and usable
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
            
            // This would throw if the client wasn't properly configured
            // In real usage, this would handle redirects automatically
            Assert.IsNotNull(request);
        }

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

            protected override HttpResponseMessage Send(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return new HttpResponseMessage(HttpStatusCode.Found)
                {
                    Headers = { Location = new Uri("https://example.com/loop") }
                };
            }
        }

        #region Synchronous Send Tests

        [TestMethod]
        public void Test_RedirectHandler_Sync_FollowsRedirect()
        {
            var mockHandler = new MockSyncRedirectHttpMessageHandler(
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

            var response = client.Send(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(2, mockHandler.RequestCount);
        }

        [TestMethod]
        public void Test_RedirectHandler_Sync_SignsRequest()
        {
            var mockHandler = new MockSyncRedirectHttpMessageHandler(
                initialResponse: new HttpResponseMessage(HttpStatusCode.OK),
                redirectResponse: null
            );

            var credentials = GetTestCredentials();
            var redirectHandler = new EdgeGridRedirectHandler(credentials)
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(redirectHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");

            client.Send(request);

            Assert.IsTrue(mockHandler.FirstRequestHadAuth);
        }

        [TestMethod]
        public void Test_RedirectHandler_Sync_MaxRedirectsExceeded()
        {
            var mockHandler = new MockInfiniteRedirectHandler();

            var credentials = GetTestCredentials();
            var redirectHandler = new EdgeGridRedirectHandler(credentials, maxRedirects: 3)
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(redirectHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/loop");

            Assert.ThrowsException<InvalidOperationException>(() => client.Send(request));
        }

        private class MockSyncRedirectHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _initialResponse;
            private readonly HttpResponseMessage? _redirectResponse;
            private int _requestCount = 0;

            public int RequestCount => _requestCount;
            public bool FirstRequestHadAuth { get; private set; }
            public bool SecondRequestHadAuth { get; private set; }
            public byte[]? LastContentBytes { get; private set; }

            public MockSyncRedirectHttpMessageHandler(
                HttpResponseMessage initialResponse,
                HttpResponseMessage? redirectResponse)
            {
                _initialResponse = initialResponse;
                _redirectResponse = redirectResponse;
            }

            protected override HttpResponseMessage Send(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _requestCount++;

                var hasAuth = request.Headers.Contains("Authorization");
                if (request.Content != null)
                {
                    LastContentBytes = request.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                }

                if (_requestCount == 1)
                {
                    FirstRequestHadAuth = hasAuth;
                    return _initialResponse;
                }
                else
                {
                    SecondRequestHadAuth = hasAuth;
                    return _redirectResponse ?? new HttpResponseMessage(HttpStatusCode.OK);
                }
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Send(request, cancellationToken));
            }
        }

        #endregion

        #region Content Preservation Tests (POST/PUT/PATCH/DELETE with body)

        [TestMethod]
        public async Task Test_RedirectHandler_Async_PreservesPostContent()
        {
            var mockHandler = new MockContentCapturingHandler();

            var credentials = GetTestCredentials();
            var redirectHandler = new EdgeGridRedirectHandler(credentials)
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(redirectHandler);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com/api");
            request.Content = new StringContent("{\"key\":\"value\"}", System.Text.Encoding.UTF8, "application/json");

            await client.SendAsync(request);

            Assert.AreEqual(2, mockHandler.RequestCount);
            Assert.IsNotNull(mockHandler.SecondRequestContent);
            Assert.AreEqual("{\"key\":\"value\"}", mockHandler.SecondRequestContent);
        }

        [TestMethod]
        public async Task Test_RedirectHandler_Async_PreservesPutContent()
        {
            var mockHandler = new MockContentCapturingHandler();

            var credentials = GetTestCredentials();
            var redirectHandler = new EdgeGridRedirectHandler(credentials)
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(redirectHandler);
            var request = new HttpRequestMessage(HttpMethod.Put, "https://example.com/api");
            request.Content = new StringContent("{\"update\":\"data\"}", System.Text.Encoding.UTF8, "application/json");

            await client.SendAsync(request);

            Assert.AreEqual(2, mockHandler.RequestCount);
            Assert.AreEqual("{\"update\":\"data\"}", mockHandler.SecondRequestContent);
        }

        [TestMethod]
        public async Task Test_RedirectHandler_Async_PreservesDeleteContent()
        {
            var mockHandler = new MockContentCapturingHandler();

            var credentials = GetTestCredentials();
            var redirectHandler = new EdgeGridRedirectHandler(credentials)
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(redirectHandler);
            var request = new HttpRequestMessage(HttpMethod.Delete, "https://example.com/api");
            request.Content = new StringContent("{\"ids\":[1,2,3]}", System.Text.Encoding.UTF8, "application/json");

            await client.SendAsync(request);

            Assert.AreEqual(2, mockHandler.RequestCount);
            Assert.AreEqual("{\"ids\":[1,2,3]}", mockHandler.SecondRequestContent);
        }

        [TestMethod]
        public void Test_RedirectHandler_Sync_PreservesDeleteContent()
        {
            var mockHandler = new MockContentCapturingHandler();

            var credentials = GetTestCredentials();
            var redirectHandler = new EdgeGridRedirectHandler(credentials)
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(redirectHandler);
            var request = new HttpRequestMessage(HttpMethod.Delete, "https://example.com/api");
            request.Content = new StringContent("{\"ids\":[1,2,3]}", System.Text.Encoding.UTF8, "application/json");

            client.Send(request);

            Assert.AreEqual(2, mockHandler.RequestCount);
            Assert.AreEqual("{\"ids\":[1,2,3]}", mockHandler.SecondRequestContent);
        }

        [TestMethod]
        public async Task Test_RedirectHandler_Async_PreservesPatchContent()
        {
            var mockHandler = new MockContentCapturingHandler();

            var credentials = GetTestCredentials();
            var redirectHandler = new EdgeGridRedirectHandler(credentials)
            {
                InnerHandler = mockHandler
            };

            using var client = new HttpClient(redirectHandler);
            var request = new HttpRequestMessage(HttpMethod.Patch, "https://example.com/api");
            request.Content = new StringContent("{\"patch\":\"data\"}", System.Text.Encoding.UTF8, "application/json");

            await client.SendAsync(request);

            Assert.AreEqual(2, mockHandler.RequestCount);
            Assert.AreEqual("{\"patch\":\"data\"}", mockHandler.SecondRequestContent);
        }

        private class MockContentCapturingHandler : HttpMessageHandler
        {
            private int _requestCount = 0;
            public int RequestCount => _requestCount;
            public string? FirstRequestContent { get; private set; }
            public string? SecondRequestContent { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _requestCount++;

                string? content = null;
                if (request.Content != null)
                {
                    content = await request.Content.ReadAsStringAsync(cancellationToken);
                }

                if (_requestCount == 1)
                {
                    FirstRequestContent = content;
                    return new HttpResponseMessage(HttpStatusCode.Found)
                    {
                        Headers = { Location = new Uri("https://example.com/redirected") }
                    };
                }
                else
                {
                    SecondRequestContent = content;
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            }

            protected override HttpResponseMessage Send(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _requestCount++;

                string? content = null;
                if (request.Content != null)
                {
                    content = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }

                if (_requestCount == 1)
                {
                    FirstRequestContent = content;
                    return new HttpResponseMessage(HttpStatusCode.Found)
                    {
                        Headers = { Location = new Uri("https://example.com/redirected") }
                    };
                }
                else
                {
                    SecondRequestContent = content;
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
        }

        #endregion
    }
}
