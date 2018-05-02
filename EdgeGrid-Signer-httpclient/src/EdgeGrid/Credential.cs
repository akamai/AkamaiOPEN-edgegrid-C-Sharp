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
            _clientToken = clientToken;
            _accessToken = accessToken;
            _clientSecret = clientSecret;
        }

        public Credential(string clientToken, string accessToken, string clientSecret, string host, string maxSize)
        {
            _clientToken = string.IsNullOrWhiteSpace(clientToken) ? string.Empty : clientToken;
            _accessToken = string.IsNullOrWhiteSpace(accessToken) ? string.Empty : accessToken;
            _clientSecret = string.IsNullOrWhiteSpace(clientSecret) ? string.Empty : clientSecret;
            _host = string.IsNullOrWhiteSpace(host) ? string.Empty : host;
            _maxSize = string.IsNullOrWhiteSpace(maxSize) ? string.Empty : maxSize;
        }

        public string ClientToken => _clientToken;

        public string AccessToken => _accessToken;

        public string ClientSecret => _clientSecret;

        public string Host => _host;

        public string MaxSize => _maxSize;

        public bool IsValid
        {
            get
            {
                bool AreCredentialsNull = string.IsNullOrWhiteSpace(_clientToken) &&
                                          string.IsNullOrWhiteSpace(_accessToken) &&
                                          string.IsNullOrWhiteSpace(_clientSecret);
                return !AreCredentialsNull;
            }
        }
    }
}