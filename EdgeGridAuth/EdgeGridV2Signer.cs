// Copyright 2025 Akamai Technologies http://developer.akamai.com.
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
// Author: smacleod@akamai.com (Stuart Macleod)
//
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Akamai.EdgeGrid.Auth
{

    /// <summary>
    /// The EdgeGrid Signer is responsible for brokering a requests.This class is responsible
    /// for the core interaction logic given an API command and the associated set of parameters.
    /// </summary>
    public class EdgeGridV2Signer
    {
        /// <summary>
        /// Name of the authorization header for signing.
        /// </summary>
        public const string AuthorizationHeader = "Authorization";

        /// <summary>
        /// The maximum body size used for computing the POST body hash (in bytes).
        /// </summary>
	    internal long? MaxBodyHashSize {get; private set; } = 131072; // 128KB

        /// <summary>
        /// Formats authorization elements into the correct order for signing
        /// </summary>
        /// <param name="credential">EdgeGrid credentials</param>
        /// <param name="timestamp">The current epoch time</param>
        internal string GetAuthDataValue(EdgeGridCredentials credential, string timestamp)
        {
            Guid nonce = Guid.NewGuid();
            return string.Format("{0} client_token={1};access_token={2};timestamp={3};nonce={4};",
                "EG1-HMAC-SHA256",
                credential.ClientToken,
                credential.AccessToken,
                timestamp,
                nonce.ToString().ToLower());
        }

        /// <summary>
        /// Formats request elements into the correct order for signing
        /// </summary>
        /// <param name="credential">EdgeGrid credentials</param>
        /// <param name="method">HTTP request method</param>
        /// <param name="pathAndQuery">The path and query string of the current HTTP request</param>
        /// <param name="requestBody">The body of the HTTP request, if any</param>
        internal string GetStringToSign(EdgeGridCredentials credential, string method, string pathAndQuery, Byte[]? requestBody = null)
        {
            string bodyHash = "";
            if (method == "POST" && requestBody != null)
            {
                bodyHash = GetRequestBodyHash(requestBody);
                Console.WriteLine("Body Hash: {0}", bodyHash);
            }

            return string.Format("{0}\t{1}\t{2}\t{3}\t\t{4}\t",
                method,
                "https",
                credential.Host,
                pathAndQuery,
                bodyHash);
        }

        /// <summary>
        /// Creates a hash of the request body for signing.
        /// </summary>
        /// <param name="requestBody">The body of the HTTP request</param>
        internal string GetRequestBodyHash(Byte[] requestBody)
        {
            if (requestBody.Length == 0) return string.Empty;

            if (requestBody.Length > MaxBodyHashSize)
            {
                // If the request body is larger than the max size, truncate it
                requestBody = requestBody.Take((int)MaxBodyHashSize).ToArray();
            }

            SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(requestBody);
            string hashString = Convert.ToBase64String(hashBytes);
            return hashString;
        }

        /// <summary>
        /// Creates a hash of of a byte array using HMACSHA256 and returns it as a base64 string. HMAC is created using the provided secret if present.
        /// </summary>
        /// <param name="data">Data to hash</param>
        /// <param name="secret">Optional secret to use when creating HMAC</param>
        internal string HashByteArray(byte[] data, string? secret = null)
        {
            // Create byte array of secret, if present
            HMACSHA256 hmac;
            if (secret != null)
            {
                byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
                hmac = new(secretBytes);
            }
            else
            {
                hmac = new();
            }

            // Hash and convert to base64 string
            byte[] hashBytes = hmac.ComputeHash(data);
            string hashString = Convert.ToBase64String(hashBytes);
            return hashString;
        }

        /// <summary>
        /// Creates a base64-encoded HMACSHA256 encrypted message from the provided message and secret.
        /// </summary>
        /// <param name="secret">Optional secret to use when creating HMAC</param>
        /// <param name="message">string to be hashed</param>
        internal string GetEncryptedMessage(string secret, string message)
        {
            // Encrypt the message using the secret key
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // Create byte array of secret
            HMACSHA256 sha256Hash;
            if (secret != null)
            {
                byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
                sha256Hash = new(secretBytes);
            }
            else
            {
                sha256Hash = new();
            }

            // Hash and convert to base64 string
            byte[] hashBytes = sha256Hash.ComputeHash(messageBytes);
            string hashString = Convert.ToBase64String(hashBytes);
            return hashString;
        }

        /// <summary>
        /// Signs the given HttpRequestMethod with the given client credential.
        /// </summary>
        /// <param name="request">The web request to sign</param>
        /// <param name="credential">the credential used in the signing</param>
        /// <returns>the signed request</returns>
        public HttpRequestMessage Sign(HttpRequestMessage request, EdgeGridCredentials credential)
        {
            if (request.RequestUri == null)
            {
                throw new ArgumentNullException(nameof(request.RequestUri), "Request URI cannot be null.");
            }

            byte[] requestBodyByteArray;
            if (request.Content == null)
                requestBodyByteArray = [];
            else
                requestBodyByteArray = request.Content.ReadAsByteArrayAsync().Result;

            string AuthHeader = GetAuthHeader(credential: credential, method: request.Method.ToString().ToUpperInvariant(), pathAndQuery: request.RequestUri.PathAndQuery, requestBodyByteArray);
            request.Headers.Add("Authorization", AuthHeader);
            return request;
        }

        /// <summary>
        /// Constructs the authorization header for the request using the provided credentials.
        /// </summary>
        /// <param name="credential">the credential used in the signing</param>
        /// <param name="method">HTTP request method</param>
        /// <param name="pathAndQuery">The path and query string of the current HTTP request</param>
        /// <param name="requestBody">The body of the HTTP request, if any</param>
        /// <returns>the signed request</returns>
        public string GetAuthHeader(EdgeGridCredentials credential, string method, string pathAndQuery, Byte[] requestBody)
        {
            // Throw an exception if the credential is null
            if (credential.ClientSecret == null || credential.ClientSecret == "")
            {
                throw new ArgumentException("ClientSecret is required for signing.");
            }

            // Get current epoch time in ISO 8601 format
            DateTime Timestamp = DateTime.UtcNow;
            string ISOTimestamp = Timestamp.ToUniversalTime().ToString("yyyyMMddTHH:mm:sszz00");

            // Construct signing string from request elements
            string RequestData = GetStringToSign(credential: credential, method: method, pathAndQuery: pathAndQuery, requestBody: requestBody);
            Console.WriteLine("Request Data: {0}", RequestData);

            // Construct auth data
            string AuthData = GetAuthDataValue(credential: credential, timestamp: ISOTimestamp);

            // Get signing key by hashing the client secret with the timestamp
            string SigningKey = GetEncryptedMessage(secret: credential.ClientSecret, message: ISOTimestamp);

            // Create the signature by hashing the request data with the signing key
            string Signature = GetEncryptedMessage(secret: SigningKey, message: $"{RequestData}{AuthData}");

            // Combine elements into Auth header value
            string AuthHeader = $"{AuthData}signature={Signature}";

            return AuthHeader;
        }
    }
}
