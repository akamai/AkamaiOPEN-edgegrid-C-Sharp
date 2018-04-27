using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using AkamaiEdgeGrid.EdgeGrid;

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
        private Dictionary<string, string> apiHeaders;
        private int MaxBodySize;
        private HttpRequestMessage HttpRequest;

        public EdgeGridSigner()
        {
            this.HttpRequest = new HttpRequestMessage();
        }

        public EdgeGridSigner(HttpMethod method, string requestUrl)
        {
            if (method == null)
            {
                throw new ArgumentException("Invalid method parameter");
            }
            else
            {
                if (String.IsNullOrWhiteSpace(requestUrl))
                {
                    throw new ArgumentException("Invalid requestUrl parameter");
                }
                else
                {
                    this.HttpRequest = new HttpRequestMessage
                    {
                        Method = method
                    };
                    this.SetRequestURI(requestUrl);
                }
            }
        }

        public EdgeGridSigner(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentException("Invalid request parameter");
            }
            else
            {
                this.HttpRequest = request;
            }
        }

        public void SetRequestURI(string request)
        {
            if (String.IsNullOrWhiteSpace(request))
            {
                throw new ArgumentException("Invalid request parameter");
            }
            else
            {
                Uri requestUri = new Uri(request);
                this.HttpRequest.RequestUri = requestUri;
            }
        }

        public void SetHttpMethod(HttpMethod method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("Invalid method parameter");
            }
            else
            {
                this.HttpRequest.Method = method;
            }
        }

        public HttpRequestMessage GetRequestMessage()
        {
            this.GenerateSignedRequest();
            return this.HttpRequest;
        }

        private void ValidateData()
        {
            if (Timestamp == null)
            {
                this.Timestamp = new EdgeGridTimestamp();
            }
            if (this.Nonce == null)
            {
                this.Nonce = new EdgeGridNonce();
            }
            if (this.HttpRequest.RequestUri == null)
            {
                throw new ArgumentNullException("Request URL not Set");
            }

            if (this.Host != null)
            {
                if (this.HttpRequest.RequestUri.Host != this.Host)
                {
                    throw new Exception.EdgeGridSignerException("URI Request does not match the HOST from the loaded Credentials");
                }
            }

            if (this.HttpRequest.Method == HttpMethod.Post || this.HttpRequest.Method == HttpMethod.Put)
            {
                if (this.HttpRequest.Content == null)
                {
                    throw new Exception.EdgeGridSignerException("Body content not set for this message request");
                }
            }

            this.ValidateClientData();//TODO check if this is necessary here, iam not sure
        }

        private void ValidateClientData()
        {
            if (String.IsNullOrWhiteSpace(this.ClientToken))
            {
                throw new ArgumentException("ClientToken is null or has not been loaded");
            }

            if (String.IsNullOrWhiteSpace(this.AccessToken))
            {
                throw new ArgumentException("AccessToken is null or has not been loaded");
            }

            if (String.IsNullOrWhiteSpace(this.ClientSecret))
            {
                throw new ArgumentException("ClientSecret is null or has not been loaded");
            }
        }

        private void GenerateSignedRequest()
        {
            string completeAuthHeader = this.GenerateAuthorizationString();
            this.HttpRequest.Headers.Add("Authorization", completeAuthHeader);
        }

        public string GenerateAuthorizationString()
        {
            this.ValidateData();
            string authHeader = string.Format(AUTHENTICATIONHEADER, ALGORITHM, ClientToken, AccessToken, this.Timestamp.ToString(), this.Nonce.ToString());
            string signingKey = GetEncryptedHMACSHA256(this.Timestamp.ToString(), this.ClientSecret);
            string dataToSign = GenerateDataToSign(authHeader);
            string signedData = GetEncryptedHMACSHA256(dataToSign, signingKey);
            string completeAuthHeader = authHeader + string.Format(SIGNATURE, signedData);
            return completeAuthHeader;
        }

        private string GenerateDataToSign(string AuthHeader)
        {
            StringBuilder SBdataToSign = new StringBuilder(CANONALIZEDHEADER);

            string requestMethod = this.HttpRequest.Method.Method.ToUpper();
            SBdataToSign = SBdataToSign.Replace("{METHOD}", requestMethod);

            string scheme = this.HttpRequest.RequestUri.Scheme.ToLower();
            SBdataToSign = SBdataToSign.Replace("{SCHEME}", scheme);

            string requestHost = this.HttpRequest.RequestUri.Host.ToLower();
            SBdataToSign = SBdataToSign.Replace("{HOST}", requestHost);

            string pathAndQuery = CheckAndCorrectPathQuery(this.HttpRequest.RequestUri.PathAndQuery);
            SBdataToSign = SBdataToSign.Replace("{QUERY}", pathAndQuery);

            string canonicalizedHeader = GetCanonicalizedHeaders();
            SBdataToSign = SBdataToSign.Replace("{HEADERS}", canonicalizedHeader);

            string contentHashBody = GetContentHash(this.HttpRequest);
            SBdataToSign = SBdataToSign.Replace("{BODY}", contentHashBody);

            SBdataToSign = SBdataToSign.Replace("{AUTHENTICATION}", AuthHeader);

            return SBdataToSign.ToString();
        }

        public void SetCredential(string ClientToken, string AccessToken, string ClientSecret)
        {
            this.ClientToken = ClientToken;
            this.AccessToken = AccessToken;
            this.ClientSecret = ClientSecret;
            //set the default body max size to 131072
            this.MaxBodySize = MAXBODYSIZE;

            this.ValidateClientData();
        }

        public void SetCredential(Credential credential)
        {
            this.ClientToken = credential.ClientToken;
            this.AccessToken = credential.AccessToken;
            this.ClientSecret = credential.ClientSecret;

            this.ValidateClientData();

            if (!string.IsNullOrWhiteSpace(credential.Host))
            {
                this.Host = credential.Host;
            }

            if (!string.IsNullOrWhiteSpace(credential.MaxSize))
            {
                int maxSizeNumber;
                bool isConvertable = Int32.TryParse(credential.MaxSize, out maxSizeNumber);
                if (isConvertable)
                {
                    this.MaxBodySize = maxSizeNumber;
                }
                else
                {
                    throw new Exception.EdgeGridSignerException("Could not convert credential.MaxSize to int");
                }
            }
            else
            {
                //set the default body max size to 131072
                this.MaxBodySize = MAXBODYSIZE;
            }
        }

        public void GetCredentialsFromEnvironment(string section = "default")
        {
            Credential environmentCredential = EnvironmentCredentialReader.GetCredential(section);
            this.SetCredential(environmentCredential);
        }

        public void GetCredentialsFromEdgerc(string section = "default", string filePath = ".edgerc")
        {
            Credential fileCredential = EdgeFileReader.CreateFromEdgeRcFile(section, filePath);
            this.SetCredential(fileCredential);
        }

        public void SetApiCustomHeaders(Dictionary<string, string> customHeaders)
        {
            if (customHeaders.Count == 0)
            {
                throw new ArgumentException("Invalid customHeaders parameter maybe is empty");
            }
            else
            {
                this.apiHeaders = customHeaders;
            }
        }

        public void AddApiCustomHeaders(string name, string value)
        {
            if (this.apiHeaders == null)
            {
                this.apiHeaders = new Dictionary<string, string>();
            }
            this.apiHeaders.Add(name, value);
        }

        private string GetEncryptedHMACSHA256(string message, string secret)
        {
            var encoding = new UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
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
            StringBuilder customHeaders = new StringBuilder();

            if (apiHeaders != null)
            {
                List<string> headerNames = new List<string>(apiHeaders.Keys);
                headerNames.Sort();
                foreach (string key in headerNames)
                {
                    string value = apiHeaders[key];
                    if (!string.IsNullOrEmpty(value))
                        customHeaders.AppendFormat("{0}:{1}\t", key.ToLower(), Regex.Replace(value.Trim(), "\\s+", " ", RegexOptions.Compiled));
                }
            }
            return customHeaders.ToString();
        }

        public void SetBodyContent(string bodyContent)
        {
            this.HttpRequest.Content = new StringContent(bodyContent, System.Text.Encoding.UTF8, "application/json");
        }

        private string GetContentHash(HttpRequestMessage request)
        {
            String data = "";

            // only do hash for POSTs or PUTs
            if (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
            {

                HttpContent content = request.Content;
                try
                {
                    if (content != null)
                    {
                        var contentString = content.ReadAsStringAsync();
                        contentString.Wait();
                        byte[] contentBytes = Encoding.UTF8.GetBytes(contentString.Result);

                        int lengthToHash = contentBytes.Length;
                        if (lengthToHash > MaxBodySize)
                        {
                            lengthToHash = MaxBodySize;
                        }

                        byte[] hashmessage;
                        using (var sha = SHA256.Create())
                        {
                            hashmessage = sha.ComputeHash(contentBytes, 0, lengthToHash);
                        }
                        data = Convert.ToBase64String(hashmessage);
                    }
                }
                catch (IOException ioe)
                {
                    throw new Exception.EdgeGridSignerException("Failed to get content hash: failed to read content", ioe);
                }

            }

            return data;
        }

    }

}
