using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Akamai.EdgeGrid.Exception;

namespace Akamai.EdgeGrid
{
    /// <summary>
    ///  Akamai {OPEN} EdgeGrid Request Signer
    /// </summary>
    public class EdgeGridSigner
    {
        private const string AUTHENTICATIONHEADER = "{0} client_token={1};access_token={2};timestamp={3};nonce={4};";
        private const string SIGNATURE = "signature={0}";
        private const string CANONALIZEDHEADER = "{METHOD}\t{SCHEME}\t{HOST}\t{QUERY}\t{HEADERS}\t{BODY}\t{AUTHENTICATION}";
        private const string ALGORITHM = "EG1-HMAC-SHA256";
        private const int MAXBODYSIZE = 131072;
        private string ClientToken;
        private string AccessToken;
        private string ClientSecret;
        public EdgeGridTimestamp Timestamp;
        public EdgeGridNonce Nonce;
        private string Host;
        private Dictionary<string, string> ApiHeaders;
        private int MaxBodySize;
        private HttpRequestMessage HttpRequest;

        public EdgeGridSigner()
        {
            HttpRequest = new HttpRequestMessage();
        }

        public EdgeGridSigner(HttpMethod method, string requestUrl)
        {
            if (method == null)
            {
                throw new ArgumentException("Invalid method parameter");
            }

            if (String.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentException("Invalid requestUrl parameter");
            }

            HttpRequest = new HttpRequestMessage
            {
                Method = method
            };
            SetRequestUri(requestUrl);
        }

        public EdgeGridSigner(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentException("Invalid request parameter");
            }

            HttpRequest = request;
        }

        public void SetRequestUri(string request)
        {
            if (String.IsNullOrWhiteSpace(request))
            {
                throw new ArgumentException("Invalid request parameter");
            }

            Uri RequestUri = new Uri(request);
            HttpRequest.RequestUri = RequestUri;
        }

        public void SetHttpMethod(HttpMethod method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("Invalid method parameter");
            }

            HttpRequest.Method = method;
        }

        public HttpRequestMessage GetRequestMessage()
        {
            GenerateSignedRequest();
            return HttpRequest;
        }

        private void ValidateData()
        {
            if (Timestamp == null)
            {
                Timestamp = new EdgeGridTimestamp();
            }
            if (Nonce == null)
            {
                Nonce = new EdgeGridNonce();
            }
            if (HttpRequest.RequestUri == null)
            {
                throw new ArgumentNullException("Request URL not Set");
            }

            if (Host != null)
            {
                if (HttpRequest.RequestUri.Host != Host)
                {
                    throw new EdgeGridSignerException("URI Request does not match the HOST from the loaded Credentials");
                }
            }

            if (HttpRequest.Method == HttpMethod.Post || HttpRequest.Method == HttpMethod.Put)
            {
                if (HttpRequest.Content == null)
                {
                    throw new EdgeGridSignerException("Body content not set for this message request");
                }
            }

            ValidateClientData();//TODO check if this is necessary here, iam not sure
        }

        private void ValidateClientData()
        {
            if (String.IsNullOrWhiteSpace(ClientToken))
            {
                throw new ArgumentException("ClientToken is null or has not been loaded");
            }

            if (String.IsNullOrWhiteSpace(AccessToken))
            {
                throw new ArgumentException("AccessToken is null or has not been loaded");
            }

            if (String.IsNullOrWhiteSpace(ClientSecret))
            {
                throw new ArgumentException("ClientSecret is null or has not been loaded");
            }
        }

        private void GenerateSignedRequest()
        {
            string CompleteAuthHeader = GenerateAuthorizationString();
            HttpRequest.Headers.Add("Authorization", CompleteAuthHeader);
        }

        public string GenerateAuthorizationString()
        {
            ValidateData();
            string AuthHeader = string.Format(AUTHENTICATIONHEADER, ALGORITHM, ClientToken, AccessToken, Timestamp, Nonce);
            string SigningKey = GetEncryptedHMACSHA256(Timestamp.ToString(), ClientSecret);
            string DataToSign = GenerateDataToSign(AuthHeader);
            string SignedData = GetEncryptedHMACSHA256(DataToSign, SigningKey);
            string CompleteAuthHeader = AuthHeader + string.Format(SIGNATURE, SignedData);
            return CompleteAuthHeader;
        }

        private string GenerateDataToSign(string authHeader)
        {
            StringBuilder SBdataToSign = new StringBuilder(CANONALIZEDHEADER);

            string RequestMethod = HttpRequest.Method.Method.ToUpper();
            SBdataToSign = SBdataToSign.Replace("{METHOD}", RequestMethod);

            string Scheme = HttpRequest.RequestUri.Scheme.ToLower();
            SBdataToSign = SBdataToSign.Replace("{SCHEME}", Scheme);

            string RequestHost = HttpRequest.RequestUri.Host.ToLower();
            SBdataToSign = SBdataToSign.Replace("{HOST}", RequestHost);

            string PathAndQuery = CheckAndCorrectPathQuery(HttpRequest.RequestUri.PathAndQuery);
            SBdataToSign = SBdataToSign.Replace("{QUERY}", PathAndQuery);

            string CanonicalizedHeader = GetCanonicalizedHeaders();
            SBdataToSign = SBdataToSign.Replace("{HEADERS}", CanonicalizedHeader);

            string ContentHashBody = GetContentHash(HttpRequest);
            SBdataToSign = SBdataToSign.Replace("{BODY}", ContentHashBody);

            SBdataToSign = SBdataToSign.Replace("{AUTHENTICATION}", authHeader);

            return SBdataToSign.ToString();
        }

        public void SetCredential(string clientToken, string accessToken, string clientSecret)
        {
            ClientToken = clientToken;
            AccessToken = accessToken;
            ClientSecret = clientSecret;
            //set the default body max size to 131072
            MaxBodySize = MAXBODYSIZE;

            ValidateClientData();
        }

        public void SetCredential(Credential credential)
        {
            ClientToken = credential.ClientToken;
            AccessToken = credential.AccessToken;
            ClientSecret = credential.ClientSecret;

            ValidateClientData();

            if (!string.IsNullOrWhiteSpace(credential.Host))
            {
                Host = credential.Host;
            }

            if (!string.IsNullOrWhiteSpace(credential.MaxSize))
            {
                bool IsConvertable = Int32.TryParse(credential.MaxSize, out var MaxSizeNumber);
                if (IsConvertable)
                {
                    MaxBodySize = MaxSizeNumber;
                }
                else
                {
                    throw new EdgeGridSignerException("Could not convert credential.MaxSize to int");
                }
            }
            else
            {
                //set the default body max size to 131072
                MaxBodySize = MAXBODYSIZE;
            }
        }

        public void GetCredentialsFromEnvironment(string section = "default")
        {
            Credential EnvironmentCredential = EnvironmentCredentialReader.GetCredential(section);
            SetCredential(EnvironmentCredential);
        }

        public void GetCredentialsFromEdgerc(string section = "default", string filePath = ".edgerc")
        {
            Credential FileCredential = EdgeFileReader.CreateFromEdgeRcFile(section, filePath);
            SetCredential(FileCredential);
        }

        public void SetApiCustomHeaders(Dictionary<string, string> customHeaders)
        {
            if (customHeaders.Count == 0)
            {
                throw new ArgumentException("Invalid customHeaders parameter maybe is empty");
            }

            ApiHeaders = customHeaders;
        }

        public void AddApiCustomHeaders(string name, string value)
        {
            if (ApiHeaders == null)
            {
                ApiHeaders = new Dictionary<string, string>();
            }
            ApiHeaders.Add(name, value);
        }

        private string GetEncryptedHMACSHA256(string message, string secret)
        {
            var Encoding = new UTF8Encoding();
            byte[] KeyByte = Encoding.GetBytes(secret);
            byte[] MessageBytes = Encoding.GetBytes(message);
            using (var Hmacsha256 = new HMACSHA256(KeyByte))
            {
                byte[] Hashmessage = Hmacsha256.ComputeHash(MessageBytes);
                return Convert.ToBase64String(Hashmessage);
            }
        }

        private string CheckAndCorrectPathQuery(string pathQuery)
        {

            if (string.IsNullOrEmpty(pathQuery))
            {
                pathQuery = "/";
            }
            else
            {
                pathQuery = pathQuery[0] == '/' ? pathQuery : "/" + pathQuery;
            }
            return pathQuery;
        }

        private String GetCanonicalizedHeaders()
        {
            StringBuilder CustomHeaders = new StringBuilder();

            if (ApiHeaders != null)
            {
                List<string> HeaderNames = new List<string>(ApiHeaders.Keys);
                HeaderNames.Sort();
                foreach (string Key in HeaderNames)
                {
                    string Value = ApiHeaders[Key];
                    if (!string.IsNullOrEmpty(Value))
                        CustomHeaders.AppendFormat("{0}:{1}\t", Key.ToLower(), Regex.Replace(Value.Trim(), "\\s+", " ", RegexOptions.Compiled));
                }
            }
            return CustomHeaders.ToString();
        }

        public void SetBodyContent(string bodyContent)
        {
            HttpRequest.Content = new StringContent(bodyContent, Encoding.UTF8, "application/json");
        }

        private string GetContentHash(HttpRequestMessage request)
        {
            String Data = "";

            // only do hash for POSTs or PUTs
            if (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
            {
                HttpContent Content = request.Content;
                try
                {
                    if (Content != null)
                    {
                        var ContentString = Content.ReadAsStringAsync();
                        ContentString.Wait();
                        byte[] ContentBytes = Encoding.UTF8.GetBytes(ContentString.Result);

                        int LengthToHash = ContentBytes.Length;
                        if (LengthToHash > MaxBodySize)
                        {
                            LengthToHash = MaxBodySize;
                        }

                        byte[] Hashmessage;
                        using (var Sha = SHA256.Create())
                        {
                            Hashmessage = Sha.ComputeHash(ContentBytes, 0, LengthToHash);
                        }
                        Data = Convert.ToBase64String(Hashmessage);
                    }
                }
                catch (IOException IoExecption)
                {
                    throw new EdgeGridSignerException("Failed to get content hash: failed to read content", IoExecption);
                }

            }

            return Data;
        }

    }

}
