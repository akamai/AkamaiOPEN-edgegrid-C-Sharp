using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
        /// Regex for normalizing whitespace in header values.
        /// </summary>
        private static readonly Regex WhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// The maximum body size used for computing the POST body hash (in bytes).
        /// </summary>
        internal long? MaxBodyHashSize { get; private set; } = EdgeGridCredentials.DefaultMaxBody;

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
        /// <param name="request">The full HTTP request message for header access</param>
        internal string GetStringToSign(EdgeGridCredentials credential, string method, string pathAndQuery,
            Byte[]? requestBody = null, HttpRequestMessage? request = null)
        {
            string bodyHash = "";
            if (method == "POST" && requestBody != null)
            {
                bodyHash = GetRequestBodyHash(requestBody, credential.MaxBody);
                Console.WriteLine("Body Hash: {0}", bodyHash);
            }

            // Canonicalize headers if headers_to_sign is configured
            string canonicalizedHeaders = "";
            if (request != null && credential.HeadersToSign.Count > 0)
            {
                canonicalizedHeaders = CanonicalizeHeaders(request.Headers, credential.HeadersToSign);
            }

            // Get host - check for custom Host header first
            string host = credential.Host ?? "";
            if (request != null && request.Headers.Host != null)
            {
                host = request.Headers.Host;
            }

            string canonicalizedUri = request != null ? GetCanonicalizedUri(request.RequestUri!) : pathAndQuery;

            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t",
                method,
                "https",
                host,
                canonicalizedUri,
                canonicalizedHeaders,
                bodyHash);
        }

        /// <summary>
        /// Gets the canonicalized URI including path, parameters (semicolon-separated), and query string.
        /// </summary>
        /// <param name="uri">The request URI</param>
        /// <returns>Canonicalized URI string</returns>
        internal string GetCanonicalizedUri(Uri uri)
        {
            string path = uri.AbsolutePath;
            string query = uri.Query;

            // Extract path parameters if present (semicolon-separated portion)
            // Example: /path;param1=value1;param2=value2?query=value
            // .NET's Uri class doesn't parse these, so we need to extract manually
            string pathParams = "";
            int semicolonIndex = path.IndexOf(';');
            if (semicolonIndex >= 0)
            {
                // Check if there's a query string after the semicolon params
                int queryStartInPath = path.IndexOf('?', semicolonIndex);
                if (queryStartInPath >= 0)
                {
                    // Params are between semicolon and query
                    pathParams = path.Substring(semicolonIndex, queryStartInPath - semicolonIndex);
                    path = path.Substring(0, semicolonIndex) + path.Substring(queryStartInPath);
                }
                else
                {
                    // Params are at the end (no query after)
                    pathParams = path.Substring(semicolonIndex);
                    path = path.Substring(0, semicolonIndex);
                }
            }

            return path + pathParams + query;
        }

        /// <summary>
        /// Creates a hash of the request body for signing.
        /// </summary>
        /// <param name="requestBody">The body of the HTTP request</param>
        /// <param name="maxBody">Maximum body size to hash</param>
        internal string GetRequestBodyHash(Byte[] requestBody, int maxBody)
        {
            if (requestBody.Length == 0)
                return string.Empty;

            if (requestBody.Length > maxBody)
            {
                // If the request body is larger than the max size, truncate it
                requestBody = requestBody.Take(maxBody).ToArray();
            }

            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(requestBody);
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Canonicalizes headers for signing - strips extra whitespace and validates
        /// </summary>
        /// <param name="headers">Request headers</param>
        /// <param name="headersToSign">List of header names to include in signature</param>
        internal string CanonicalizeHeaders(System.Net.Http.Headers.HttpRequestHeaders headers, List<string> headersToSign)
        {
            var canonicalized = new List<string>();

            foreach (var headerName in headersToSign)
            {
                if (headers.TryGetValues(headerName, out var values))
                {
                    var headerValue = string.Join(",", values);

                    // Validate no leading whitespace (unless quoted)
                    if (!headerValue.StartsWith("\"") && headerValue.TrimStart() != headerValue)
                    {
                        throw new InvalidOperationException(
                            $"Invalid leading whitespace, reserved character(s), or return character(s) in header value: '{headerValue}'");
                    }

                    // Normalize whitespace
                    var normalized = WhitespaceRegex.Replace(headerValue.Trim(), " ");
                    canonicalized.Add($"{headerName}:{normalized}");
                }
            }

            return string.Join("\t", canonicalized);
        }

        /// <summary>
        /// Adds Akamai CLI version headers to User-Agent if environment variables are set
        /// </summary>
        internal void AddVersionHeaders(HttpRequestMessage request)
        {
            var versionBuilder = new StringBuilder();

            var akamaiCli = Environment.GetEnvironmentVariable("AKAMAI_CLI");
            var akamaiCliVersion = Environment.GetEnvironmentVariable("AKAMAI_CLI_VERSION");
            if (!string.IsNullOrEmpty(akamaiCli) && !string.IsNullOrEmpty(akamaiCliVersion))
            {
                versionBuilder.Append(" AkamaiCLI/").Append(akamaiCliVersion);
            }

            var akamaiCliCommand = Environment.GetEnvironmentVariable("AKAMAI_CLI_COMMAND");
            var akamaiCliCommandVersion = Environment.GetEnvironmentVariable("AKAMAI_CLI_COMMAND_VERSION");
            if (!string.IsNullOrEmpty(akamaiCliCommand) && !string.IsNullOrEmpty(akamaiCliCommandVersion))
            {
                versionBuilder.Append(" AkamaiCLI-").Append(akamaiCliCommand).Append('/').Append(akamaiCliCommandVersion);
            }

            if (versionBuilder.Length > 0)
            {
                string versionHeader = versionBuilder.ToString();
                if (request.Headers.UserAgent.Count == 0)
                {
                    request.Headers.TryAddWithoutValidation("User-Agent", versionHeader.Trim());
                }
                else
                {
                    var currentUserAgent = string.Join(" ", request.Headers.UserAgent);
                    request.Headers.Remove("User-Agent");
                    request.Headers.TryAddWithoutValidation("User-Agent", currentUserAgent + versionHeader);
                }
            }
        }

        /// <summary>
        /// Creates a hash of of a byte array using HMACSHA256 and returns it as a base64 string. HMAC is created using the provided secret if present.
        /// </summary>
        /// <param name="data">Data to hash</param>
        /// <param name="secret">Optional secret to use when creating HMAC</param>
        internal string HashByteArray(byte[] data, string? secret = null)
        {
            using var hmac = secret != null
                ? new HMACSHA256(Encoding.UTF8.GetBytes(secret))
                : new HMACSHA256();

            byte[] hashBytes = hmac.ComputeHash(data);
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Creates a base64-encoded HMACSHA256 encrypted message from the provided message and secret.
        /// </summary>
        /// <param name="secret">Optional secret to use when creating HMAC</param>
        /// <param name="message">string to be hashed</param>
        internal string GetEncryptedMessage(string secret, string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] secretBytes = Encoding.UTF8.GetBytes(secret);

            using var sha256Hash = new HMACSHA256(secretBytes);
            byte[] hashBytes = sha256Hash.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Creates an HttpClient with automatic redirect handling and EdgeGrid authentication.
        /// </summary>
        /// <param name="credential">EdgeGrid credentials for signing</param>
        /// <param name="maxRedirects">Maximum number of redirects to follow (default: 10)</param>
        /// <returns>An HttpClient configured with EdgeGrid redirect handling</returns>
        public static HttpClient CreateHttpClient(EdgeGridCredentials credential, int maxRedirects = 10)
        {
            var redirectHandler = new EdgeGridRedirectHandler(credential, maxRedirects)
            {
                InnerHandler = new HttpClientHandler
                {
                    AllowAutoRedirect = false // We handle redirects manually
                }
            };

            return new HttpClient(redirectHandler);
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

            // Add version headers
            AddVersionHeaders(request);

            byte[] requestBodyByteArray;
            if (request.Content == null)
                requestBodyByteArray = [];
            else
                requestBodyByteArray = request.Content.ReadAsByteArrayAsync().Result;

            string AuthHeader = GetAuthHeader(credential: credential, method: request.Method.ToString().ToUpperInvariant(),
                pathAndQuery: request.RequestUri.PathAndQuery, requestBodyByteArray, request);
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
        /// <param name="request">The full HTTP request message for header access</param>
        /// <returns>the signed request</returns>
        public string GetAuthHeader(EdgeGridCredentials credential, string method, string pathAndQuery,
            Byte[] requestBody, HttpRequestMessage? request = null)
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
            string RequestData = GetStringToSign(credential: credential, method: method,
                pathAndQuery: pathAndQuery, requestBody: requestBody, request: request);
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
