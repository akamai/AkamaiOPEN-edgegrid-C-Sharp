using System;
using System.Net.Http; 
using System.Net.Http.Headers; 
using System.Threading; 
using System.Threading.Tasks; 
using System.Collections.Generic; 
using System.Security.Cryptography; 
using System.Text; 

namespace Akamai.EdgeGrid
{
    public class SignHttpClient
    {
        
        private HttpClient _client;
        public SignHttpClient() {
            this._client = new HttpClient();
        } 

        public SignHttpClient(HttpClient client) {
            this._client = client;
        } 

        public Uri BaseAddress{ 
            get { return this._client.BaseAddress;}
            set { this._client.BaseAddress = value; } 
        }

        public HttpRequestHeaders DefaultRequestHeaders { 
            get { return this._client.DefaultRequestHeaders; }  
        }

        public long MaxResponseContentBufferSize { 
            get{ return this._client.MaxResponseContentBufferSize; } 
            set{ this._client.MaxResponseContentBufferSize = value; } 
        }

        public TimeSpan Timeout { 
            get{ return this._client.Timeout; } 
            set{ this._client.Timeout = value; } 
        }

        private readonly string[] authenticationHeaders = new string[] { "signatureType", "client_token", "access_token", "timestamp", "nonce" };

        internal string GetAuthDataValue(Credential credential, DateTime timestamp)
        {
            if (timestamp == null)
                throw new ArgumentNullException("timestamp cannot be null");

            Guid nonce = Guid.NewGuid();
            return string.Format("{0} client_token={1};access_token={2};timestamp={3};nonce={4};", 
                "this.SignVersion.Name",
                credential.ClientToken,
                credential.AccessToken,
                DateTime.UtcNow.ToString("yyyyMMddTHH:mm:ss+0000"),
                nonce.ToString().ToLower());
        }

        /// <summary> 
        /// Generate a nonce 
        /// </summary> 
        /// <returns></returns>       
        private string GenerateNonce() 
        { 
            return Guid.NewGuid().ToString(); 
        } 
        
        /// <summary> 
        /// Helper function to compute a hash value 
        /// </summary> 
        /// <param name="hashAlgorithm">The hashing algorithm used. If that algorithm needs some initialization, like HMAC and its derivatives, they should be initialized prior to passing it to this function</param> 
        /// <param name="data">The data to hash</param> 
        /// <returns>a Base64 string of the hash value</returns> 
        private string ComputeHash(HashAlgorithm hashAlgorithm, string data) 
        { 
            if (hashAlgorithm == null) 
            { 
                throw new ArgumentNullException("hashAlgorithm"); 
            } 
 
            if (string.IsNullOrEmpty(data)) 
            { 
                throw new ArgumentNullException("data"); 
            } 
 
            byte[] dataBuffer = System.Text.Encoding.ASCII.GetBytes(data); 
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer); 
 
            return Convert.ToBase64String(hashBytes); 
        } 


    }
}