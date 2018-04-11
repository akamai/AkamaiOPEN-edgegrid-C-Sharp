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

		public Credential(string clientToken, string accessToken, string clientSecret)
		{
            this._clientToken = clientToken;
			this._accessToken = accessToken;
			this._clientSecret = clientSecret;
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

        public bool isValid{
            get {
            bool areCredentialsNull = string.IsNullOrWhiteSpace(this._clientToken) && string.IsNullOrWhiteSpace(this._accessToken) && string.IsNullOrWhiteSpace(this._clientSecret);
            return !areCredentialsNull;
            }
        }

    }
}