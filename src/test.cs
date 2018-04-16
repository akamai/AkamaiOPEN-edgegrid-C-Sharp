using System;
using Akamai;
using Akamai.EdgeGrid;
using System.Threading;
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

        string Host = "akab-nncvkmov5ebjtmy5-wszz3cqirmefgkn7.luna.akamaiapis.net";
        string resource = "/diagnostic-tools/v2/ghost-locations/available";
        string requestUrl = "https://" + Host + resource;
        
        Akamai.EdgeGrid.AuthenticationHeader test = new Akamai.EdgeGrid.AuthenticationHeader();
        test.getCredentialsFromEnvironment();
        test.setRequestURI(requestUrl);
        test.setHttpMethod(HttpMethod.Get);
        Task request = testRequest(test);
        request.Wait();
        
        
        Akamai.EdgeGrid.AuthenticationHeader test2 = new Akamai.EdgeGrid.AuthenticationHeader(HttpMethod.Get,requestUrl);
        test.getCredentialsFromEnvironment();
        Task request2 = testRequest(test);
        request2.Wait();


        Akamai.EdgeGrid.AuthenticationHeader test3 = new Akamai.EdgeGrid.AuthenticationHeader(HttpMethod.Get,requestUrl);
        test.getCredentialsFromEdgerc("papi", "/Users/miguel.chang/Documents/akamai/auth.edgerc");
        Task request3 = testRequest(test3);
        request3.Wait();

        Akamai.EdgeGrid.Credential credential = new Akamai.EdgeGrid.Credential(ClientToken, AccessToken, ClientSecret);
            bool areCredentialsValid = credential.isValid;
        }

        private static async Task<string> testRequest(Akamai.EdgeGrid.AuthenticationHeader AuthHeader) {
            string sresponse =string.Empty;
            try{
                var response = await client.SendAsync(AuthHeader.GetRequestMessage());
                sresponse = await response.Content.ReadAsStringAsync();
            }catch (Exception ex){

             string here = ex.Message;
            }
            return sresponse;
        }


    }
}
