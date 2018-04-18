using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Akamai.EdgeGrid.Exception;

namespace Akamai.EdgeGrid
{
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
        private string _timestamp;
        public string Timestamp {
            get {return _timestamp;}
            set {_timestamp = value;}
        }
        private string _nonce;

        public string Nonce {
            get {return _nonce;}
            set {_nonce = value;}   
        }
        private string Host;


        private Dictionary<string, string> apiHeaders;

        private int _maxBodySize;

        private HttpRequestMessage _httpRequest;

        public EdgeGridSigner (){
            _httpRequest = new HttpRequestMessage();
        }

        public EdgeGridSigner(HttpMethod method,string requestUrl){
            this._httpRequest = new HttpRequestMessage();
            this._httpRequest.Method = method;
            this.setRequestURI(requestUrl);
        }

        public EdgeGridSigner(HttpRequestMessage request){
            this._httpRequest = request;
        }

        public void setRequestURI(string request){
            Uri requestUri = new Uri(request);
            this._httpRequest.RequestUri = requestUri; 
        }

        public void setHttpMethod(HttpMethod method){
            this._httpRequest.Method = method; 
        }
        public HttpRequestMessage GetRequestMessage(){
            this.generateSignedRequest();
            return _httpRequest;
        }

        private void validateData(){
            if (Timestamp == null){
                this.Timestamp = this.GetCurrentTimeStamp();
            }
            if (this.Nonce == null){
                this.Nonce = this.GenerateNonce();
            }
            if(this._httpRequest.RequestUri == null){
                throw new System.Exception("Request URL not Set");
            }

            if(this.Host != null){
                if (this._httpRequest.RequestUri.Host != this.Host){
                        throw new System.Exception("URI Request does not match the HOST from the loaded Credentials");
                }
            }
            
            if(String.IsNullOrWhiteSpace(this.ClientToken)){
                throw new System.Exception("ClientToken is null or has not been loaded");
            }

            if(String.IsNullOrWhiteSpace(this.AccessToken)){
                throw new System.Exception("AccessToken is null or has not been loaded");
            }

            if(String.IsNullOrWhiteSpace(this.ClientSecret)){
                throw new System.Exception("ClientSecret is null or has not been loaded");
            }

            if (this._httpRequest.Method == HttpMethod.Post || this._httpRequest.Method == HttpMethod.Put){
                if (this._httpRequest.Content == null) {
                    throw new Akamai.EdgeGrid.Exception.EdgeGridSignerException("Body content not set for this message request");                    
                }
            }

        }

        public void generateSignedRequest(){
            this.validateData();
            string authHeader = string.Format(AUTHENTICATIONHEADER, ALGORITHM , ClientToken , AccessToken , Timestamp, Nonce);
            string signingKey = getEncryptedHMACSHA256(this.Timestamp, this.ClientSecret);
            string dataToSign = generateDataToSign(authHeader);
            string signedData = getEncryptedHMACSHA256(dataToSign,signingKey);
            string completeAuthHeader = authHeader + string.Format(SIGNATURE, signedData);
             _httpRequest.Headers.Add("Authorization",completeAuthHeader);
        }

        public string generateAuthorizationString(){
            this.validateData();
            string authHeader = string.Format(AUTHENTICATIONHEADER, ALGORITHM , ClientToken , AccessToken , Timestamp, Nonce);
            string signingKey = getEncryptedHMACSHA256(this.Timestamp, this.ClientSecret);
            string dataToSign = generateDataToSign(authHeader);
            string signedData = getEncryptedHMACSHA256(dataToSign,signingKey);
            string completeAuthHeader = authHeader + string.Format(SIGNATURE, signedData);
            return completeAuthHeader;
        }
        private string generateDataToSign(string AuthHeader){
            StringBuilder SBdataToSign = new StringBuilder(CANONALIZEDHEADER);

            string requestMethod = _httpRequest.Method.Method.ToUpper();
            SBdataToSign = SBdataToSign.Replace("{METHOD}", requestMethod);  

            string scheme = _httpRequest.RequestUri.Scheme.ToLower();
            SBdataToSign = SBdataToSign.Replace("{SCHEME}", scheme);  

            string requestHost = _httpRequest.RequestUri.Host.ToLower();
            SBdataToSign = SBdataToSign.Replace("{HOST}", requestHost);  
            
            string pathAndQuery = checkAndCorrectPathQuery(_httpRequest.RequestUri.PathAndQuery);
            SBdataToSign = SBdataToSign.Replace("{QUERY}", pathAndQuery);  

            string canonicalizedHeader = getCanonicalizedHeaders();
            SBdataToSign = SBdataToSign.Replace("{HEADERS}", canonicalizedHeader);  

            string contentHashBody = GetContentHash(_httpRequest);
            SBdataToSign = SBdataToSign.Replace("{BODY}", contentHashBody);  

            SBdataToSign = SBdataToSign.Replace("{AUTHENTICATION}", AuthHeader);  

            string dataToSign = SBdataToSign.ToString();
            return dataToSign;
        }

        public void setCredential(string ClientToken, string AccessToken, string ClientSecret){
            this.ClientToken = ClientToken;
            this.AccessToken = AccessToken;
            this. ClientSecret = ClientSecret;
            //set the default body max size to 131072
            this._maxBodySize = MAXBODYSIZE;
        }

        public void setCredential(Credential credential){
            this.ClientToken = credential.ClientToken;
            this.AccessToken = credential.AccessToken;
            this.ClientSecret = credential.ClientSecret;

            if (!string.IsNullOrWhiteSpace(credential.Host)){
                this.Host = credential.Host;
            }

            if (!string.IsNullOrWhiteSpace(credential.MaxSize)){
                int maxSizeNumber;
                bool isConvertable = Int32.TryParse(credential.MaxSize, out maxSizeNumber);
                if (isConvertable){
                    this._maxBodySize = maxSizeNumber;
                }else{
                    throw new Akamai.EdgeGrid.Exception.EdgeGridSignerException("Could not convert credential.MaxSize to int");
                }
                
            }else{
                //set the default body max size to 131072
                this._maxBodySize = 131072;
            }
        }

        public void getCredentialsFromEnvironment(string section = "default"){
            Credential environmentCredential=EnvironmentCredentialReader.getCredential(section);
            this.setCredential(environmentCredential);
        }

        public void getCredentialsFromEdgerc(string section = "default", string filePath = ".edgerc"){
           Credential fileCredential = EdgeFileReader.CreateFromEdgeRcFile(section,filePath);
           this.setCredential(fileCredential);
        }

        public void setApiCustomHeaders(Dictionary<string, string> customHeaders){
            this.apiHeaders = customHeaders;
        }

        public void addApiCustomHeaders( string name, string value ){
            if(this.apiHeaders == null){
                this.apiHeaders = new Dictionary<string, string>();
            }
            this.apiHeaders.Add(name, value);
        }

        private string getEncryptedHMACSHA256(string message, string secret)
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
            
            if(apiHeaders != null){
                List<string> headerNames = new List<string> (apiHeaders.Keys);
                headerNames.Sort();
                foreach( string key in headerNames){
                    string value = apiHeaders[key];
                    if (!string.IsNullOrEmpty(value))
                        customHeaders.AppendFormat("{0}:{1}\t", key.ToLower(), Regex.Replace(value.Trim(), "\\s+", " ", RegexOptions.Compiled));
                }
            }
            return customHeaders.ToString();
		}

        public void setBodyContent(string bodyContent) {
            this._httpRequest.Content = new StringContent(bodyContent, System.Text.Encoding.UTF8, "application/json");
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
                        if(lengthToHash > _maxBodySize ){
                            lengthToHash = _maxBodySize;
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
					throw new Akamai.EdgeGrid.Exception.EdgeGridSignerException("Failed to get content hash: failed to read content", ioe);
				}					

			}
            return data;
		}


    }

}
