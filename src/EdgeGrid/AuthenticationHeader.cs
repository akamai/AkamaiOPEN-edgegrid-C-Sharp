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

        private const string HEADER = "{0} client_token={1};access_token={2};timestamp={3};nonce={4};"; 
        private string CANONALIZEDHEADER = "{METHOD}\t{SCHEME}\t{HOST}\t{QUERY}\t{HEADERS}\t{BODY}\t{AUTHENTICATION}";
        private string headerSignature = "signature={0}";

        private Dictionary<string, string> apiHeaders;

        HttpRequestMessage _httpRequest;
        HttpHeaders _httpHeader;

        public AuthenticationHeader (){

            this.Timestamp = this.GetCurrentTimeStamp();
            this.Nonce = this.GenerateNonce();
            string resource = "/api-definitions/v2/endpoints/user-entitlements";
            string requestUrl = "https://" + Host + resource;

            _httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUrl );

            Uri uri = _httpRequest.RequestUri;
            
            string requestMethod = _httpRequest.Method.Method.ToUpper();
            string scheme = uri.Scheme.ToLower();
            string requestHost = uri.Host.ToLower();
            string pathAndQuery = uri.PathAndQuery;
            string canonicalizedHeader = "booo";
            string contentHashPost = "sf";

            string authHeader = string.Format(HEADER, Algorithm , ClientToken , AccessToken , Timestamp, Nonce);
            string signingKey = getEncryptedSHA256(this.Timestamp, this.ClientSecret);

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
			return DateTime.Now.ToString("yyyyMMdd'T'HH:mm:ssZ");
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
