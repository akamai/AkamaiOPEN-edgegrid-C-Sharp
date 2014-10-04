// Copyright 2014 Akamai Technologies http://developer.akamai.com.
//
// Licensed under the Apache License, KitVersion 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Author: colinb@akamai.com  (Colin Bendell)
//

using Akamai.EdgeGrid.Auth;
using Akamai.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Akamai.EdgeGrid
{
    /// <summary>
    /// Command Line sample application to demonstrate the utilization of the {Open} APIs. 
    /// This can be used for both command line invocation or reference on how to leverage the 
    /// Api. All supported commands are implemented in this sample for convience.
    /// 
    /// Author: colinb@akamai.com  (Colin Bendell)
    /// </summary>
    class OpenAPI
    {
        static void Main(string[] args)
        {
            string secret = null;
            string clientToken = null;
            string accessToken = null;
            string apiurl = null;
            List<string> headers = new List<string>();
            bool isGetMethod = true;
            string contentType = "application/json";


            string outputfile = null;
            string uploadfile = null;
            string data = null;
           
            string firstarg = null;
            foreach (string arg in args)
            {
                if (firstarg != null)
                {
                    switch (firstarg)
                    {
                        case "-a":
                            accessToken = arg;
                            break;
                        case "-c":
                            clientToken = arg;
                            break;
                        case "-s":
                            secret = arg;
                            break;
                        case "-o":
                            outputfile = arg;
                            break;
                        case "-f":
                            isGetMethod = false;
                            uploadfile = arg;
                            break;
                        case "-d":
                            isGetMethod = false;
                            data = arg;
                            break;
                        case "-T":
                            contentType = arg;
                            break;
                        case "-H":
                            headers.Add(arg);
                            break;
                        
                    }
                    firstarg = null;
                }
                else if (arg == "-h" || arg == "--help" || arg == "/?")
                {
                    help();
                    return;
                }
                else if (arg == "-P")
                    isGetMethod = false;
                else if (!arg.StartsWith("-"))
                    apiurl = arg;
                else
                    firstarg = arg;
            }


            execute(isGetMethod, apiurl, headers, clientToken, accessToken, secret, data, uploadfile, outputfile, contentType);
        }

        static void execute(bool isGetMethod, string apiurl, List<string> headers, string clientToken, string accessToken, string secret, string data, string uploadfile, string outputfile, string contentType)
        {
            if (apiurl == null || clientToken == null || accessToken == null || secret == null)
            {
                help();
                return;
            }
            
            EdgeGridV1Signer signer = new EdgeGridV1Signer();
            ClientCredential credential = new ClientCredential(clientToken, accessToken, secret);

            Stream uploadStream = null;
            if (uploadfile != null)
                uploadStream = new FileInfo(uploadfile).OpenRead();
            else if (data != null)
                uploadStream = new MemoryStream(data.ToByteArray());

            var uri = new Uri(apiurl);
            var request = WebRequest.Create(uri);

            if (uploadStream != null)
            {
                isGetMethod = false;
                request.ContentType = contentType;
            }

            foreach (string header in headers) request.Headers.Add(header);
            request.Method = isGetMethod ? "GET" : "POST";

            Stream output = Console.OpenStandardOutput();
            if (outputfile != null)
                output = new FileInfo(outputfile).OpenWrite();
              
            using (var result = signer.Execute(request, credential, uploadStream))
            {
                using (output)
                {
                    using (result)
                    {
                        byte[] buffer = new byte[32*1024];
                        int bytesRead = 0;

                        while ((bytesRead = result.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            output.Write(buffer, 0, bytesRead);
                        }
                    }
                }
            }
        


        }

        static void help()
        {
            Console.Error.WriteLine(@"
Usage: openapi <-c client-token> <-a access-token> <-s secret>
           [-d data] [-f srcfile]
           [-o outfile] [-P]
           [-H header-line]
           [-T content-type]
           <url>

Where:
    -o outfile      local file name to use to save response from the API
    -d data         string of data to PUT to the API
    -f srcfile      local file used as source when action=upload
    -H header-line  Http Header 'Name: value'
    -P              force HTTP PUT 
    -T content-type the HTTP content type (default = application/json)
    url             fully qualified api url such as https://akab-1234.luna.akamaiapis.net/diagnostic-tools/v1/locations       

");
        }
    }
}
