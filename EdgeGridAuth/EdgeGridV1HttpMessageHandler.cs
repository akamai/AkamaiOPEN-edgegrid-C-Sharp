// Copyright 2014 Akamai Technologies http://developer.akamai.com.
//
// Licensed under the Apache License, KitVersion 2.0 (the "License");
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
//
// Author: colinb@akamai.com  (Colin Bendell)
//
using Akamai.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http.Headers;

namespace Akamai.EdgeGrid.Auth
{

    /// <summary>
    /// The EdgeGrid Signer is responsible for brokering a requests.This class is responsible 
    /// for the core interaction logic given an API command and the associated set of parameters.
    /// 
    /// When event is executed, the 'Authorization' header is decorated
    /// 
    /// If connection is going to be reused, pass the persistent HttpWebRequest object when calling execute()
    /// 
    /// TODO: support rebinding on IO communication errors (eg: connection reset)
    /// TODO: support Async callbacks and Async IO
    /// TODO: support multiplexing 
    /// TODO: optimize and adapt throughput based on connection latency
    /// 
    /// Author: colinb@akamai.com  (Colin Bendell)
    /// </summary>
    public class EdgeGridV1HttpMessageHandler : HttpClientHandler
    {

        public class SignType
        {
            public static SignType HMACSHA256 = new SignType("EG1-HMAC-SHA256", KeyedHashAlgorithm.HMACSHA256);

            public string Name { get; private set; }
            public KeyedHashAlgorithm Algorithm { get; private set; }
            private SignType(string name, KeyedHashAlgorithm algorithm)
            {
                this.Name = name;
                this.Algorithm = algorithm;
            }
        }

        public class HashType
        {
            public static HashType SHA256 = new HashType(ChecksumAlgorithm.SHA256);

            public ChecksumAlgorithm Checksum { get; private set; }
            private HashType(ChecksumAlgorithm checksum)
            {
                this.Checksum = checksum;
            }
        }

        public const string AuthorizationHeader = "Authorization";

        /// <summary>
        /// The SignVersion enum value
        /// </summary>
        internal SignType SignVersion {get; private set;}

        /// <summary>
        /// The checksum mechanism to hash the request body
        /// </summary>
        internal HashType HashVersion {get; private set;}

        /// <summary>
        /// The ordered list of header names to include in the signature.
        /// </summary>
        internal IList<string> HeadersToInclude { get; private set; }

        /// <summary>
        /// The maximum body size used for computing the POST body hash (in bytes).
        /// </summary>
	    internal long? MaxBodyHashSize {get; private set; }

        readonly ClientCredential _creds;

        public EdgeGridV1HttpMessageHandler(ClientCredential creds, IList<string> headers = null, long? maxBodyHashSize = 2048)
        {
            this.HeadersToInclude = headers ?? new List<string>();
            this.MaxBodyHashSize = maxBodyHashSize;
            this.SignVersion = SignType.HMACSHA256;
            this.HashVersion = HashType.SHA256;
            _creds = creds;
        }

        internal string GetAuthDataValue(ClientCredential credential, DateTime timestamp)
        {
            if (timestamp == null)
                throw new ArgumentNullException("timestamp cannot be null");

            Guid nonce = Guid.NewGuid();
            return string.Format("{0} client_token={1};access_token={2};timestamp={3};nonce={4};", 
                this.SignVersion.Name,
                credential.ClientToken,
                credential.AccessToken,
                timestamp.ToISO8601(),
                nonce.ToString().ToLower());
        }

        internal string GetRequestData(string method, Uri uri, HttpRequestHeaders requestHeaders = null, Stream requestStream = null)
        {
            if (string.IsNullOrEmpty(method))
                throw new ArgumentNullException("Invalid request: empty request method");

            String headers = GetRequestHeaders(requestHeaders);
            String bodyHash = "";
            // Only POST body is hashed
            if (method == "POST")
                bodyHash = GetRequestStreamHash(requestStream);

            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t",
                method.ToUpper(),
                uri.Scheme,
                uri.Host,
                uri.PathAndQuery,
                headers,
                bodyHash);
        }

        internal string GetRequestHeaders(HttpRequestHeaders requestHeaders)
        {
            if (requestHeaders == null) return string.Empty;

            StringBuilder headers = new StringBuilder();
            foreach (string name in this.HeadersToInclude)
            {
                //TODO: should auto detect headers and remove standard non-http headers
                string value = String.Join("", requestHeaders.GetValues(name));
                if (!string.IsNullOrEmpty(value))
                    headers.AppendFormat("{0}:{1}\t", name, Regex.Replace(value.Trim(), "\\s+", " ", RegexOptions.Compiled));
            }
            return headers.ToString();
        }

        internal string GetRequestStreamHash(Stream requestStream)
        {
            if (requestStream == null) return string.Empty;

            if (!requestStream.CanRead)
                throw new IOException("Cannot read stream to compute hash");

            if (!requestStream.CanSeek)
                throw new IOException("Stream must be seekable!");

            string streamHash = requestStream.ComputeHash(this.HashVersion.Checksum, MaxBodyHashSize).ToBase64();
            requestStream.Seek(0, SeekOrigin.Begin);
            return streamHash;
        }

        internal string GetAuthorizationHeaderValue(ClientCredential credential, DateTime timestamp, string authData, string requestData)
        {
            string signingKey = timestamp.ToISO8601().ToByteArray().ComputeKeyedHash(credential.Secret, this.SignVersion.Algorithm).ToBase64();
            string authSignature = string.Format("{0}{1}", requestData, authData).ToByteArray().ComputeKeyedHash(signingKey, this.SignVersion.Algorithm).ToBase64();
            return string.Format("{0}signature={1}", authData, authSignature);
        }

        /// <summary>
        /// Validates the response and attempts to detect root causes for failures for non 200 responses. The most common cause is 
        /// due to time synchronization of the local server. If the local server is more than 30seconds out of sync then the 
        /// API server will reject the request.
        /// 
        /// TODO: catch rate limitting errors. Should delay and retry.
        /// </summary>
        /// <param name="response">the active response object</param>
        public void Validate(WebResponse response)
        {
            if (response is HttpWebResponse)
            {
                HttpWebResponse httpResponse = (HttpWebResponse)response;
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                    return;

                DateTime responseDate;
                string date = response.Headers.Get("Date");
                if (date != null
                    && DateTime.TryParse(date, out responseDate))
                    if (DateTime.Now.Subtract(responseDate).TotalSeconds > 30)
                        throw new HttpRequestException("Local server Date is more than 30s out of sync with Remote server");

                string responseBody = null;
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseBody = reader.ReadToEnd();
                    // Do something with the value
                }
               
                throw new HttpRequestException(string.Format("Unexpected Response from Server: {0} {1}\n{2}\n\n{3}", httpResponse.StatusCode, httpResponse.StatusDescription, response.Headers, responseBody));
            }
        }
                
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            DateTime timestamp = DateTime.UtcNow;
            Stream uploadStream = request.Content == null ? null : await request.Content.ReadAsStreamAsync();

            //already signed?
            if (request.Headers.Contains(EdgeGridV1Signer.AuthorizationHeader))
                request.Headers.Remove(EdgeGridV1Signer.AuthorizationHeader);

            string requestData = GetRequestData(request.Method.Method, request.RequestUri, request.Headers, uploadStream);
            string authData = GetAuthDataValue(_creds, timestamp);
            string authHeader = GetAuthorizationHeaderValue(_creds, timestamp, authData, requestData);
            request.Headers.Add(EdgeGridV1Signer.AuthorizationHeader, authHeader);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
