using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Akamai.EdgeGrid.Auth;

namespace Akamai.EdgeGrid
{
    /// <summary>
    /// Command-line sample application to demonstrate the use of the {Open} APIs. 
    /// This can be used both for command-line invocation or as a reference on how to leverage the 
    /// APIs. All supported commands are implemented in this sample for convenience.
    /// </summary>
    class EdgeGridConsole
    {
        static void Main(string[] args)
        {
            string edgeRCFile = "";
            string section = "";
            string ask = "";
            string path = "";
            List<string> headers = [];
            string method = "GET";
            string contentType = "application/json";


            string outputfile = "";
            string uploadfile = "";
            string data = "";

            bool verbose = false;

            string? firstarg = null;
            foreach (string arg in args)
            {
                if (firstarg != null)
                {
                    switch (firstarg)
                    {
                        case "-p":
                            path = arg;
                            break;
                        case "-e":
                            edgeRCFile = arg;
                            break;
                        case "-s":
                            section = arg;
                            break;
                        case "-a":
                            ask = arg;
                            break;
                        case "-d":
                            if (method == "GET")
                                method = "POST";
                            data = arg;
                            break;
                        case "-f":
                            if (method == "GET")
                                method = "PUT";
                            uploadfile = arg;
                            break;
                        case "-H":
                            headers.Add(arg);
                            break;
                        case "-o":
                            outputfile = arg;
                            break;
                        case "-T":
                            contentType = arg;
                            break;
                        case "-X":
                            method = arg;
                            break;

                    }
                    firstarg = null;
                }
                else if (arg == "-h" || arg == "--help" || arg == "/?")
                {
                    Help();
                    return;
                }
                else if (arg == "-v" || arg == "-vv")
                    verbose = true;
                else if (!arg.StartsWith("-"))
                    path = arg;
                else
                    firstarg = arg;
            }

            if (verbose)
            {
                Console.WriteLine("{0} {1}", method, path);
                Console.WriteLine("EdgeRCFile: {0}", edgeRCFile);
                Console.WriteLine("Section: {0}", section);
                if (data != null)
                    Console.WriteLine("Data: [{0}]", data);
                if (uploadfile != null)
                    Console.WriteLine("UploadFile: {0}", uploadfile);
                if (outputfile != null)
                    Console.WriteLine("OutputFile: {0}", outputfile);
                foreach (string header in headers)
                    Console.WriteLine("{0}", header);
                Console.WriteLine("Content-Type: {0}", contentType);
            }

            Execute(method: method, path: path, headers: headers, edgeRCFile: edgeRCFile, section: section, accountSwitchKey: ask, data: data, uploadfile: uploadfile, outputfile: outputfile, contentType: contentType, verbose: verbose);
        }

        static void Execute(string method, string path, List<string> headers, string edgeRCFile, string section, string accountSwitchKey, string? data, string? uploadfile, string? outputfile, string contentType, bool verbose = false)
        {
            if (path == null)
            {
                Help();
                return;
            }

            EdgeGridCredentials credentials = new(edgeRCFile, section);

            // Add an account switch key to a path if provided
            if (!string.IsNullOrEmpty(accountSwitchKey))
            {
                if (path.Contains("?"))
                {
                    path += "&accountSwitchKey=" + accountSwitchKey;
                }
                else
                {
                    path += "?accountSwitchKey=" + accountSwitchKey;
                }
            }

            var uri = new Uri($"https://{credentials.Host}{path}");
            var request = new HttpRequestMessage(new HttpMethod(method), uri);

            if (uploadfile != null && uploadfile != "")
            {
                FileStream stream = File.OpenRead(uploadfile);
                request.Content = new StreamContent(stream);
            }

            else if (data != null && data != "")
            {
                HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
                request.Content = content;
            }

            foreach (string header in headers)
            {
                var components = header.Split(':', 2);
                request.Headers.Add(components[0], components[1]);
            }

            // Default headers
            if (request.Headers.Accept == null || !request.Headers.Accept.Any())
            {
                request.Headers.Add("accept", "application/json");
            }
            if (request.Headers.UserAgent == null || !request.Headers.UserAgent.Any())
            {
                request.Headers.Add("user-agent", "EdgeGridConsole");
            }

            // Create a client with automatic signing and redirect handling
            HttpClient client = EdgeGridV2Signer.CreateHttpClient(credentials);
            HttpResponseMessage response = client.Send(request);

            Console.WriteLine("{0} {1}", (int)response.StatusCode, response.ReasonPhrase ?? "No Reason");
            Console.WriteLine(response.Headers.ToString());
            string responseBody = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(responseBody);
        }

        static void Help()
        {
            Console.Error.WriteLine(@"
Usage: openapi <-e edgerc-file> <-s section> <-a account-switch-key>
           [-d data] [-f srcfile]
           [-o outfile]
           [-m max-size]
           [-X method]
           [-H header-line]
           [-T content-type]
           <url>

Where:
    -o outfile      Local file name to use to save the response from the API
    -d data         String of data to PUT to the API
    -f srcfile      Local file used as source when action=upload
    -m max-size     Maximum amount of data to use in the signing hash. Default is 2048
    -H header-line  HTTP Header 'Name: value'
    -X method       Force HTTP PUT, POST, DELETE
    -T content-type The HTTP content type (default = application/json)
    url             Fully qualified API URL such as https://akab-1234.luna.akamaiapis.net/identity-management/v3/user-profile

");
        }
    }
}
