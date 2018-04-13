using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Akamai.EdgeGrid
{
    public class AuthenticationHeader
    {
        private string Algorithm;
        private string ClientToken;
        private string AccessToken;
        private string Timestamp;
        private string Nonce;
        private string Signature;
        
        private string ClientSecret;

        private string Host;

        private const string AUTHENTICATIONHEADER = "{0} client_token={1};access_token={2};timestamp={3};nonce={4};"; 
        private const string SIGNATURE = "signature={0}";
        private const string CANONALIZEDHEADER = "{METHOD}\t{SCHEME}\t{HOST}\t{QUERY}\t{HEADERS}\t{BODY}\t{AUTHENTICATION}";


        private Dictionary<string, string> apiHeaders;

        private int _maxBodySize;

        HttpRequestMessage _httpRequest;
        HttpHeaders _httpHeader;

        public AuthenticationHeader (){
            this.Algorithm = "EG1-HMAC-SHA256";
            this.ClientToken = "akab-ykktmjocsjg5pv6v-eazbp6if7sdz4muc";
            this.AccessToken = "akab-2l3wh7tumycg4buw-hmmhsuqp4qxbf6b6";
            this.Host = "akab-nncvkmov5ebjtmy5-wszz3cqirmefgkn7.luna.akamaiapis.net";
            this.ClientSecret = "zQtOSUvjY5YPs953NIXNkvJQ3jDfyWCKLs9uGhnWatE=";
            this.Timestamp = this.GetCurrentTimeStamp();
            this.Nonce = this.GenerateNonce();
            //string resource = "/diagnostic-tools/v2/ghost-locations/available";
            string resource = "/diagnostic-tools/v2/end-users/diagnostic-url";
            string requestUrl = "https://" + Host + resource;

            //_httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUrl );
            _httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl );
            _httpRequest.Content = new StringContent("{\"url\":\"http://www.test.com\",\"url\": \"www.test.com\"}", Encoding.UTF8, "application/json");
            apiHeaders = new Dictionary<string, string> ();
            Uri uri = _httpRequest.RequestUri;
            
            string authHeader = string.Format(AUTHENTICATIONHEADER, Algorithm , ClientToken , AccessToken , Timestamp, Nonce);
            string signingKey = getEncryptedSHA256(this.Timestamp, this.ClientSecret);

            StringBuilder SBdataToSign = new StringBuilder(CANONALIZEDHEADER);

            string requestMethod = _httpRequest.Method.Method.ToUpper();
            SBdataToSign = SBdataToSign.Replace("{METHOD}", requestMethod);  

            string scheme = uri.Scheme.ToLower();
            SBdataToSign = SBdataToSign.Replace("{SCHEME}", scheme);  

            string requestHost = uri.Host.ToLower();
            SBdataToSign = SBdataToSign.Replace("{HOST}", requestHost);  
            
            string pathAndQuery = uri.PathAndQuery;
            SBdataToSign = SBdataToSign.Replace("{QUERY}", pathAndQuery);  

            string canonicalizedHeader = getCanonicalizedHeaders();
            SBdataToSign = SBdataToSign.Replace("{HEADERS}", canonicalizedHeader);  

            string contentHashBody = getHashedBody();
            contentHashBody = getHashedSHA256Body(contentHashBody);

            SBdataToSign = SBdataToSign.Replace("{BODY}", contentHashBody);  

            SBdataToSign = SBdataToSign.Replace("{AUTHENTICATION}", authHeader);  

            string dataToSign = SBdataToSign.ToString();

            string dataSigned = getEncryptedSHA256(dataToSign,signingKey);

            string completeAuthHeader = authHeader + string.Format(SIGNATURE, dataSigned);

            _httpRequest.Headers.Add("Authorization",completeAuthHeader);


            }

        public HttpRequestMessage GetRequestMessage(){

            return _httpRequest;

        }

        private string checkAndCorrectPathQuery(string pathQuery) {
			if (string.IsNullOrEmpty(pathQuery))
			{
				pathQuery = "/";
			}else{
                pathQuery = pathQuery[0] == '/' ? pathQuery :"/" + pathQuery;
            }
            return pathQuery;
        }

        //private string canonicalize() {}

		private string GetCurrentTimeStamp()
		{   
			return DateTime.UtcNow.ToString("yyyyMMddTHH:mm:ss+0000");
		}
        
        private string getEncryptedSHA256(string message, string secret)
        {
        var encoding = new System.Text.UTF8Encoding();
        byte[] keyByte = encoding.GetBytes(secret);
        byte[] messageBytes = encoding.GetBytes(message);
        using (var hmacsha256 = new HMACSHA256(keyByte))
        {
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashmessage);
        }
        }

        private string getHashedBody(){
            
            string requestBody = string.Empty;

            if (_httpRequest != null) {
                if(_httpRequest.Content != null)
                    requestBody = getHashedSHA256Body(_httpRequest.Content.ToString());
            }
            return requestBody;

        }

        private string getHashedSHA256Body(string body)
        {
        var encoding = new System.Text.UTF8Encoding();
        byte[] messageBytes = encoding.GetBytes(body);
        using (var hmacsha256 = new HMACSHA256())
        {
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashmessage);
        }
        }

        /// <summary> 
        /// Generate a nonce 
        /// </summary> 
        /// <returns></returns>       
        private string GenerateNonce() 
        { 
            return Guid.NewGuid().ToString(); 
        } 

        private String getCanonicalizedHeaders()
		{
			StringBuilder customHeaders = new StringBuilder();
            
            List<string> headerNames = new List<string> (apiHeaders.Keys);
            headerNames.Sort();
            
            foreach( string key in headerNames){
                //TODO: should auto detect headers and remove standard non-http headers
                string value = apiHeaders[key];
                if (!string.IsNullOrEmpty(value))
                    customHeaders.AppendFormat("{0}:{1}\t", key.ToLower(), Regex.Replace(value.Trim(), "\\s+", " ", RegexOptions.Compiled));
            }
            return customHeaders.ToString();

		}
    }

}
