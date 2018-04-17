using System;
using Akamai;
using Akamai.EdgeGrid;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

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
        string postResource = "/diagnostic-tools/v2/end-users/diagnostic-url";
        string requestUrl = "https://" + Host + resource;
        //string postBody = "{\n    \"endUserName\": \"name\",\n    \"url\": \"www.test.com\"\n}";
        dynamic body = new JObject();
        body.endUserName = "name";
        body.url = "www.test.com";
        string postBody = "{\n    \"endUserName\": \"name\",\n    \"url\": \"www.test.com\"\n}";
        postBody = body.ToString();
                                
        string postRequestUrl = "https://" + Host + postResource;

        Akamai.EdgeGrid.AuthenticationHeader testPost = new Akamai.EdgeGrid.AuthenticationHeader();
        testPost.getCredentialsFromEnvironment();
        testPost.setRequestURI(postRequestUrl);
        testPost.setHttpMethod(HttpMethod.Post);
        testPost.setBodyContent(postBody);
        HttpRequestMessage messageRequest = testPost.GetRequestMessage();
        Task requestPost = testMessageRequest(messageRequest);

        requestPost.Wait();

        Akamai.EdgeGrid.AuthenticationHeader test = new Akamai.EdgeGrid.AuthenticationHeader();
        test.getCredentialsFromEnvironment();
        test.setRequestURI(requestUrl);
        test.setHttpMethod(HttpMethod.Get);
        Task request = testRequest(test);
        request.Wait();
        
        
        Akamai.EdgeGrid.AuthenticationHeader test2 = new Akamai.EdgeGrid.AuthenticationHeader(HttpMethod.Get,requestUrl);
        test2.getCredentialsFromEnvironment();
        Task request2 = testRequest(test);
        request2.Wait();


        Akamai.EdgeGrid.AuthenticationHeader test3 = new Akamai.EdgeGrid.AuthenticationHeader(HttpMethod.Get,requestUrl);
        test3.getCredentialsFromEdgerc("default", "/Users/miguel.chang/Documents/akamai/auth.edgerc");
        Task request3 = testRequest(test3);
        request3.Wait();

        Akamai.EdgeGrid.Credential credential = new Akamai.EdgeGrid.Credential(ClientToken, AccessToken, ClientSecret);
            bool areCredentialsValid = credential.isValid;
        }

        private static async Task<string> testMessageRequest(HttpRequestMessage requestMessage) {
            string sresponse =string.Empty;
            try{
                string request = requestMessage.ToString();
                string headers = requestMessage.Headers.ToString();
                string content = await requestMessage.Content.ReadAsStringAsync();
                var response = await client.SendAsync(requestMessage);
                sresponse = await response.Content.ReadAsStringAsync();
            }catch (Exception ex){

             string here = ex.Message;
            }
            return sresponse;
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
