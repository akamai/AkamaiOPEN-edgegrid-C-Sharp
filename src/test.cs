using System;
using Akamai;
using Akamai.EdgeGrid;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;

namespace Akamai
{
    public class test
    {
        private static readonly HttpClient client = new HttpClient();

        public static void Main(string[] args){
		//ProcessRepositories().Wait();

        string ClientToken = "lala";
		string AccessToken = "mema"; 
		string ClientSecret = "dej9d";        
        
        Dictionary<string, string> headers;

        headers = new Dictionary<string,string>();

        headers.Add("X-Some-Signed-Header", "header value");
        headers.Add("X-Some-Signed-Header", "header value");

        Akamai.EdgeGrid.AuthenticationHeader test = new Akamai.EdgeGrid.AuthenticationHeader();
        Akamai.EdgeGrid.Credential credentialEnv = Akamai.EdgeGrid.CredentialFactory.CreateFromEnvironment();
        Akamai.EdgeGrid.Credential credentialFile = Akamai.EdgeGrid.CredentialFactory.CreateFromEdgeRcFile("default", "/Users/miguel.chang/Documents/akamai/auth.edgerc");
        Akamai.EdgeGrid.Credential credential = new Akamai.EdgeGrid.Credential(ClientToken, AccessToken, ClientSecret);

            bool areCredentialsValid = credential.isValid;
            var serializer = new DataContractJsonSerializer(typeof(Akamai.EdgeGrid.Credential));

        }

        private static async Task ProcessRepositories()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var stringTask = client.GetStringAsync("https://api.github.com/orgs/dotnet/repos");

            var msg = await stringTask;
            Console.Write(msg);
        }

    }
}
