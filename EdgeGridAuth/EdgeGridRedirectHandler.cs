// Copyright 2025 Akamai Technologies http://developer.akamai.com.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#nullable enable
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Akamai.EdgeGrid.Auth
{
    /// <summary>
    /// HTTP message handler that automatically follows redirects and resigns requests
    /// with EdgeGrid authentication headers.
    /// </summary>
    public class EdgeGridRedirectHandler : DelegatingHandler
    {
        private readonly EdgeGridCredentials _credentials;
        private readonly EdgeGridV2Signer _signer;
        private readonly int _maxRedirects;

        /// <summary>
        /// Initializes a new instance of the EdgeGridRedirectHandler class.
        /// </summary>
        /// <param name="credentials">EdgeGrid credentials for signing</param>
        /// <param name="maxRedirects">Maximum number of redirects to follow (default: 10)</param>
        public EdgeGridRedirectHandler(EdgeGridCredentials credentials, int maxRedirects = 10)
        {
            _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            _signer = new EdgeGridV2Signer();
            _maxRedirects = maxRedirects;
        }

        /// <summary>
        /// Sends an HTTP request with automatic redirect handling and resigning.
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            int redirectCount = 0;
            HttpResponseMessage? response = null;

            while (redirectCount <= _maxRedirects)
            {
                // Send the request
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                // Check if response is a redirect
                if (!IsRedirect(response.StatusCode))
                {
                    return response;
                }

                // Get redirect location
                if (response.Headers.Location == null)
                {
                    return response; // No location header, return as-is
                }

                Uri redirectUri = response.Headers.Location;
                
                // Make absolute if relative
                if (!redirectUri.IsAbsoluteUri)
                {
                    redirectUri = new Uri(request.RequestUri!, redirectUri);
                }

                Console.WriteLine($"Following redirect to: {redirectUri}");

                // Create new request for redirect location
                var redirectRequest = new HttpRequestMessage(request.Method, redirectUri);

                // Copy headers from original request (except Authorization)
                foreach (var header in request.Headers)
                {
                    if (header.Key != "Authorization" && header.Key != "Host")
                    {
                        redirectRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                // Copy content for POST/PUT/PATCH requests
                if (request.Content != null && 
                    (request.Method == HttpMethod.Post || 
                     request.Method == HttpMethod.Put || 
                     request.Method == HttpMethod.Patch))
                {
                    // Clone the content
                    var contentBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                    redirectRequest.Content = new ByteArrayContent(contentBytes);
                    
                    // Copy content headers
                    foreach (var header in request.Content.Headers)
                    {
                        redirectRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                // Sign the redirect request with new auth header
                _signer.Sign(redirectRequest, _credentials);

                // Dispose previous response
                response.Dispose();

                // Update request for next iteration
                request = redirectRequest;
                redirectCount++;
            }

            throw new InvalidOperationException(
                $"Maximum number of redirects ({_maxRedirects}) exceeded.");
        }

        /// <summary>
        /// Determines if a status code represents a redirect.
        /// </summary>
        private static bool IsRedirect(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.MovedPermanently ||    // 301
                   statusCode == HttpStatusCode.Found ||                // 302
                   statusCode == HttpStatusCode.SeeOther ||             // 303
                   statusCode == HttpStatusCode.TemporaryRedirect ||    // 307
                   statusCode == HttpStatusCode.PermanentRedirect;      // 308 (.NET Core 2.0+)
        }
    }
}
