namespace Akamai.EdgeGrid
{
    public class Credential
    {
        //Akamai API Client token 
        private readonly string _clientToken;
        //Akamai API Access Token
        private readonly string _accessToken;
        //Akamai API Client Secret
        private readonly string _clientSecret;

        //Akamai API Host
        private readonly string _host;

        //Akamai API post body size
        private readonly string _maxSize;

        public Credential(string clientToken, string accessToken, string clientSecret)
        {
            this._clientToken = clientToken;
            this._accessToken = accessToken;
            this._clientSecret = clientSecret;
        }

        public Credential(string clientToken, string accessToken, string clientSecret, string host, string maxSize)
        {
            this._clientToken = string.IsNullOrWhiteSpace(clientToken) ? string.Empty : clientToken;
            this._accessToken = string.IsNullOrWhiteSpace(accessToken) ? string.Empty : accessToken;
            this._clientSecret = string.IsNullOrWhiteSpace(clientSecret) ? string.Empty : clientSecret;
            this._host = string.IsNullOrWhiteSpace(host) ? string.Empty : host;
            this._maxSize = string.IsNullOrWhiteSpace(maxSize) ? string.Empty : maxSize;
        }

        public string ClientToken
        {
            get
            {
                return this._clientToken;
            }
        }
        public string AccessToken
        {
            get
            {
                return this._accessToken;
            }
        }
        public string ClientSecret
        {
            get
            {
                return this._clientSecret;
            }
        }

        public string Host
        {
            get
            {
                return this._host;
            }
        }
        public string MaxSize
        {
            get
            {
                return this._maxSize;
            }
        }

        public bool IsValid
        {
            get
            {
                bool areCredentialsNull = string.IsNullOrWhiteSpace(this._clientToken) && string.IsNullOrWhiteSpace(this._accessToken) && string.IsNullOrWhiteSpace(this._clientSecret);
                return !areCredentialsNull;
            }
        }

    }
}