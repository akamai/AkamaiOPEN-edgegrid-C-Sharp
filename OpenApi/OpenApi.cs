using Akamai.EdgeGrid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace OpenApi
{
    /// <summary>
    /// Command Line sample application to demonstrate the utilization of the {Open} APIs. 
    /// This can be used for both command line invocation or reference on how to leverage the 
    /// Api. All supported commands are implemented in this sample for convience.
    /// 
    /// </summary>
    class OpenAPI
    {
        private readonly static HttpClient client = new HttpClient();  
        
        static void Main(string[] args)
        {

            string secret = null;
            string clientToken = null;
            string accessToken = null;
            string apiurl = null;
            List<string> headers = new List<string>();
            string httpMethod = "GET";
            HttpMethod requestMethod = HttpMethod.Get;
            bool loadCredFromEnvironment = false;
            bool loadCredFromEdgerc = false;
            bool uploadFromFile = false;
            string envCredentials = "";
            string edgercfilePath = null;
            string section = "default";
            string outputfile = null;
            string uploadfile = null;
            string data = null;

            bool verbose = false;
           
            string firstarg = null;
            foreach (string arg in args)
            {
                if (firstarg != null)
                {
                    switch (firstarg)
                    {
                        case "--a":
                            accessToken = arg;
                            break;
                        case "--c":
                            clientToken = arg;
                            break;
                        case "--d":
                            if (httpMethod == "GET"){
                                httpMethod = "POST";
                                requestMethod = HttpMethod.Post;
                            } 
                            data = arg;
                            break;
                        case "--e":
                            envCredentials = arg;
                            switch(envCredentials){
                                case "false": loadCredFromEnvironment = false;
                                break;
                                default: loadCredFromEnvironment = true;
                                break;
                            }
                            break;                    
                        case "--f":
                            if (httpMethod == "GET"){
                                httpMethod = "POST";
                                requestMethod = HttpMethod.Post;
                            } 
                            uploadfile = arg;
                            uploadFromFile = true;
                            break;
                        case "--H":
                            headers.Add(arg);
                            break;
                        case "--o":
                            outputfile = arg;
                            break;
                        case "--r":
                            loadCredFromEnvironment = false;
                            //loading credentials from edgerc file takes precedence over environment
                            loadCredFromEdgerc = true;
                            break;
                        case "--s":
                            secret = arg;
                            break;
                        case "--X":
                            httpMethod = arg;
                            switch(httpMethod) {
                                case "POST": requestMethod = HttpMethod.Post; 
                                break;
                                case "DELETE": requestMethod = HttpMethod.Delete; 
                                break;            
                                case "PUT": requestMethod = HttpMethod.Put; 
                                break;
                                case "HEAD": requestMethod = HttpMethod.Head; 
                                break; 
                                default: requestMethod = HttpMethod.Get;  
                                break;                       
                            }
                            break;

                    }
                    firstarg = null;
                }
                else if (arg == "-h" || arg == "--help" || arg == "/?")
                {
                    help();
                    return;
                }
                else if (arg == "-v" || arg == "-vv")
                    verbose = true;           
                else if (!arg.StartsWith("-"))
                    apiurl = arg;
                else
                    firstarg = arg;
            }

            if (verbose)
            {
                Console.WriteLine("{0} {1}", httpMethod, apiurl);
                Console.WriteLine("ClientToken: {0}", clientToken);
                Console.WriteLine("AccessToken: {0}", accessToken);
                Console.WriteLine("Secret: {0}", secret);
                if (data != null) Console.WriteLine("Data: [{0}]", data);
                if (uploadfile != null) Console.WriteLine("UploadFile: {0}", uploadfile);
                if (outputfile != null) Console.WriteLine("OutputFile: {0}", outputfile);
                foreach (string header in headers)
                Console.WriteLine("{0}", header);
            }

            if(apiurl == null){
                Console.WriteLine("No URL defined");
                help();
                return;
            }
            if ((clientToken == null || accessToken == null || secret == null) && (loadCredFromEdgerc == false) && (loadCredFromEnvironment == false))
            {
                Console.WriteLine("No credentials defined");
                help();
                return;
            }

            EdgeGridSigner signer = new EdgeGridSigner( requestMethod, apiurl);

            if (!string.IsNullOrWhiteSpace(clientToken) && !string.IsNullOrWhiteSpace(accessToken) && !string.IsNullOrWhiteSpace(secret) && !string.IsNullOrWhiteSpace(apiurl)){
                signer.setCredential(clientToken, accessToken, secret);            
            }else{
                if (loadCredFromEnvironment){
                    signer.getCredentialsFromEnvironment(section);            
                }
                if (loadCredFromEnvironment){
                    signer.getCredentialsFromEdgerc(edgercfilePath,section);            
                }
            }
            
            if(requestMethod == HttpMethod.Post || requestMethod == HttpMethod.Put){
                if(!string.IsNullOrWhiteSpace(data) ){
                    signer.setBodyContent(data);
                }

                if(uploadFromFile){
                    string bodyContent = "";
                    try{    
                       bodyContent = System.IO.File.ReadAllText(uploadfile);
                    }catch(Exception io){
                        Console.WriteLine(io.Message);
                        return;
                    }
                    signer.setBodyContent(bodyContent);
                }
            }

            if(headers != null){
                if (headers.Count > 0){
                    Dictionary<string,string> dictionaryHeader = new Dictionary<string,string>();
                    foreach (string header in headers){
                       string[] keyAndValue = header.Split(':',2);
                       dictionaryHeader.Add(keyAndValue[0], keyAndValue[1]);
                    }
                signer.setApiCustomHeaders(dictionaryHeader);
                }
            }
            HttpRequestMessage request;
            try{
                request = signer.GetRequestMessage();
            }catch(Exception ex){
                Console.WriteLine(ex.Message);
                return;
            }

            if (verbose)
            {
                Console.WriteLine("Authorization: {0}", request.Headers.Authorization);
                Console.WriteLine();
            }
            var response = client.SendAsync(request);
            response.Wait();


            var responseContent = response.Result.Content.ReadAsStringAsync();
            responseContent.Wait();

            string result = responseContent.Result;

            if(string.IsNullOrWhiteSpace(outputfile)){
                Console.WriteLine("Request response: {0}" , responseContent.ToString());
            }else{
                try{
                    System.IO.File.WriteAllText(outputfile, responseContent.ToString());
                }catch(Exception ex){
                    Console.WriteLine(ex.Message);
                }
            }

        }

        static void help()
        {
            Console.Error.WriteLine(@"
Usage: openapi <--c client-token> <--a access-token> <--s secret>
           [--d data] [--f srcfile]
           [--e env-credentials]
           [--o outfile]
           [--X method]
           [--H header-line]
           [--r edgerc-path]
           <url>

Where:
    --o outfile      local file name to use to save response from the API
    --d data         string of data to POST to the API
    --e env-credentials  Load credentials from environment, takes true or false. Default value is false.
    --f srcfile      local file used as source when action=upload
    --H header-line  Http Header 'Name: value'
    --X method       force HTTP PUT,POST,DELETE 
    --r edgerc-path  path to the edgrc file used for the credentials
    url              fully qualified api url is required, such as https://akab-1234.luna.akamaiapis.net/diagnostic-tools/v1/locations       
");
        }
    }
}