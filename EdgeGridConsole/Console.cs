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
    /// Command-line argument options
    /// </summary>
    public class CommandLineOptions
    {
        public string EdgeRCFile { get; set; } = "";
        public string Section { get; set; } = "";
        public string AccountSwitchKey { get; set; } = "";
        public string Path { get; set; } = "";
        public List<string> Headers { get; set; } = [];
        public string Method { get; set; } = "GET";
        public string ContentType { get; set; } = "application/json";
        public string OutputFile { get; set; } = "";
        public string UploadFile { get; set; } = "";
        public string Data { get; set; } = "";
        public bool Verbose { get; set; } = false;
        public bool ShowHelp { get; set; } = false;
    }

    /// <summary>
    /// Command-line sample application to demonstrate the use of the {Open} APIs. 
    /// This can be used both for command-line invocation or as a reference on how to leverage the 
    /// APIs. All supported commands are implemented in this sample for convenience.
    /// </summary>
    public class EdgeGridConsole
    {
        static void Main(string[] args)
        {
            var options = ParseArguments(args);

            if (options.ShowHelp)
            {
                Help();
                return;
            }

            if (options.Verbose)
            {
                Console.WriteLine("{0} {1}", options.Method, options.Path);
                Console.WriteLine("EdgeRCFile: {0}", options.EdgeRCFile);
                Console.WriteLine("Section: {0}", options.Section);
                if (options.Data != null)
                    Console.WriteLine("Data: [{0}]", options.Data);
                if (options.UploadFile != null)
                    Console.WriteLine("UploadFile: {0}", options.UploadFile);
                if (options.OutputFile != null)
                    Console.WriteLine("OutputFile: {0}", options.OutputFile);
                foreach (string header in options.Headers)
                    Console.WriteLine("{0}", header);
                Console.WriteLine("Content-Type: {0}", options.ContentType);
            }

            Execute(options);
        }

        /// <summary>
        /// Parse command-line arguments into CommandLineOptions
        /// </summary>
        public static CommandLineOptions ParseArguments(string[] args)
        {
            var options = new CommandLineOptions();
            string? firstarg = null;

            foreach (string arg in args)
            {
                if (firstarg != null)
                {
                    switch (firstarg)
                    {
                        case "-p":
                            options.Path = arg;
                            break;
                        case "-e":
                            options.EdgeRCFile = arg;
                            break;
                        case "-s":
                            options.Section = arg;
                            break;
                        case "-a":
                            options.AccountSwitchKey = arg;
                            break;
                        case "-d":
                            if (options.Method == "GET")
                                options.Method = "POST";
                            options.Data = arg;
                            break;
                        case "-f":
                            if (options.Method == "GET")
                                options.Method = "PUT";
                            options.UploadFile = arg;
                            break;
                        case "-H":
                            options.Headers.Add(arg);
                            break;
                        case "-o":
                            options.OutputFile = arg;
                            break;
                        case "-T":
                            options.ContentType = arg;
                            break;
                        case "-X":
                            options.Method = arg;
                            break;
                    }
                    firstarg = null;
                }
                else if (arg == "-h" || arg == "--help" || arg == "/?")
                {
                    options.ShowHelp = true;
                    return options;
                }
                else if (arg == "-v" || arg == "-vv")
                    options.Verbose = true;
                else if (!arg.StartsWith("-"))
                    options.Path = arg;
                else
                    firstarg = arg;
            }

            return options;
        }

        /// <summary>
        /// Add account switch key to path if provided
        /// </summary>
        public static string AddAccountSwitchKeyToPath(string path, string accountSwitchKey)
        {
            if (string.IsNullOrEmpty(accountSwitchKey))
                return path;

            if (path.Contains("?"))
            {
                return path + "&accountSwitchKey=" + accountSwitchKey;
            }
            else
            {
                return path + "?accountSwitchKey=" + accountSwitchKey;
            }
        }

        static void Execute(CommandLineOptions options)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                Help();
                return;
            }

            EdgeGridCredentials credentials = new(options.EdgeRCFile, options.Section);

            // Add an account switch key to a path if provided
            string path = AddAccountSwitchKeyToPath(options.Path, options.AccountSwitchKey);

            var uri = new Uri($"https://{credentials.Host}{path}");
            var request = new HttpRequestMessage(new HttpMethod(options.Method), uri);

            if (options.UploadFile != null && options.UploadFile != "")
            {
                FileStream stream = File.OpenRead(options.UploadFile);
                request.Content = new StreamContent(stream);
            }

            else if (options.Data != null && options.Data != "")
            {
                HttpContent content = new StringContent(options.Data, Encoding.UTF8, "application/json");
                request.Content = content;
            }

            foreach (string header in options.Headers)
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
            HttpClient client = EdgeGridSigner.CreateHttpClient(credentials);
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
