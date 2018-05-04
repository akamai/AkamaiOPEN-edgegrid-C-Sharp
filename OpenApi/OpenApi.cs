using Akamai.EdgeGrid;
using System;
using System.Collections.Generic;
using System.Net.Http;
using OpenApi.Exception;
using OpenApi.Model;

namespace OpenApi
{
    /// <summary>
    /// Command Line sample application to demonstrate the utilization of the {Open} APIs. 
    /// This can be used for both command line invocation or reference on how to leverage the 
    /// Api. All supported commands are implemented in this sample for convience.
    /// </summary>
    class OpenAPI
    {
        private static readonly HttpClient Client = new HttpClient();

        static void Main(string[] args)
        {
            string Secret = null;
            string ClientToken = null;
            string AccessToken = null;
            string ApiUrl = null;
            List<string> Headers = new List<string>();
            string StringHttpMethod = "GET";
            HttpMethod RequestMethod = HttpMethod.Get;
            bool LoadCredentialFromEnvironment = false;
            bool LoadCredentialFromEdgerc = false;
            bool UploadFromFile = false;
            string EdgercFilePath = null;
            string Section = "default";
            string OutputFile = null;
            string UploadFile = null;
            string Data = null;
            bool Verbose = false;

            try
            {
                Dictionary<OpenApiCommand.Argument, string> CommandArgumentArray =
                    OpenApiCommand.ExctratCommandArguments(args);

                if (
                    CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.Help) ||
                    CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.Help1) ||
                    CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.Help2) ||
                    CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.Help3)
                )
                {
                    Help();
                    return;
                }

                if (CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.ClientToken))
                {
                    ClientToken = CommandArgumentArray[OpenApiCommand.Argument.ClientToken];
                }

                if (CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.AccessToken))
                {
                    AccessToken = CommandArgumentArray[OpenApiCommand.Argument.AccessToken];
                }

                if (CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.Secret))
                {
                    Secret = CommandArgumentArray[OpenApiCommand.Argument.Secret];
                }

                if (CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.Data))
                {
                    Data = CommandArgumentArray[OpenApiCommand.Argument.Data];
                    RequestMethod = HttpMethod.Post;
                }

                if (CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.EnvironmentCredential))
                {
                    LoadCredentialFromEnvironment =
                        CommandArgumentArray[OpenApiCommand.Argument.EnvironmentCredential].ToUpper() ==
                        Boolean.TrueString;
                }

                if (CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.SourceFile))
                {
                    UploadFile = CommandArgumentArray[OpenApiCommand.Argument.SourceFile];
                    UploadFromFile = true;
                    RequestMethod = HttpMethod.Post;
                }

                if (CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.HeaderLine))
                {
                    Headers.Add(
                        CommandArgumentArray[
                            OpenApiCommand.Argument.HeaderLine]); //TODO this need to be proccess as separated by coma
                }

                if (CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.OutputFile))
                {
                    OutputFile = CommandArgumentArray[OpenApiCommand.Argument.OutputFile];
                }

                if (CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.EdgercPath))
                {
                    EdgercFilePath = CommandArgumentArray[OpenApiCommand.Argument.EdgercPath];
                    LoadCredentialFromEnvironment = false;
                    LoadCredentialFromEdgerc = true;
                }

                if (CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.Method))
                {
                    switch (CommandArgumentArray[OpenApiCommand.Argument.Method].ToUpper())
                    {
                        case "POST":
                            RequestMethod = HttpMethod.Post;
                            break;
                        case "DELETE":
                            RequestMethod = HttpMethod.Delete;
                            break;
                        case "PUT":
                            RequestMethod = HttpMethod.Put;
                            break;
                        case "HEAD":
                            RequestMethod = HttpMethod.Head;
                            break;
                        default:
                            RequestMethod = HttpMethod.Get;
                            break;
                    }
                }

                if (CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.Verbose) ||
                    CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.Verbose1))
                {
                    Verbose = true;
                }

                if (CommandArgumentArray.ContainsKey(OpenApiCommand.Argument.Url))
                {
                    ApiUrl = CommandArgumentArray[OpenApiCommand.Argument.Url];
                }

                /*
                 * DEsde aqui es donde comienza a ejecutarse la libreria como tal, lo que hay antes es la extraccion de los comandos
                 */

                if (Verbose)
                {
                    Console.WriteLine("{0} {1}", StringHttpMethod, ApiUrl);
                    Console.WriteLine("ClientToken: {0}", ClientToken);
                    Console.WriteLine("AccessToken: {0}", AccessToken);
                    Console.WriteLine("Secret: {0}", Secret);
                    if (Data != null) Console.WriteLine("Data: [{0}]", Data);
                    if (UploadFile != null) Console.WriteLine("UploadFile: {0}", UploadFile);
                    if (OutputFile != null) Console.WriteLine("OutputFile: {0}", OutputFile);
                    foreach (string Header in Headers)
                    {
                        Console.WriteLine("{0}", Header);
                    }
                }

                EdgeGridSigner Signer = new EdgeGridSigner(RequestMethod, ApiUrl);

                if (!string.IsNullOrWhiteSpace(ClientToken) && !string.IsNullOrWhiteSpace(AccessToken) &&
                    !string.IsNullOrWhiteSpace(Secret) && !string.IsNullOrWhiteSpace(ApiUrl))
                {
                    Signer.SetCredential(ClientToken, AccessToken, Secret);
                }
                else
                {
                    if (LoadCredentialFromEnvironment)
                    {
                        Signer.GetCredentialsFromEnvironment(Section);
                    }

                    if (LoadCredentialFromEdgerc)
                    {
                        Signer.GetCredentialsFromEdgerc(Section, EdgercFilePath);
                    }
                }

                if (RequestMethod == HttpMethod.Post || RequestMethod == HttpMethod.Put)
                {
                    if (!string.IsNullOrWhiteSpace(Data))
                    {
                        Signer.SetBodyContent(Data);
                    }

                    if (UploadFromFile)
                    {
                        string BodyContent;
                        try
                        {
                            BodyContent = System.IO.File.ReadAllText(UploadFile);
                        }
                        catch (System.Exception Ex)
                        {
                            throw new OpenApiException(Ex.Message);
                        }

                        Signer.SetBodyContent(BodyContent);
                    }
                }

                if (Headers != null)
                {
                    if (Headers.Count > 0)
                    {
                        Dictionary<string, string> DictionaryHeader = new Dictionary<string, string>();
                        foreach (string Header in Headers)
                        {
                            string[] KeyAndValue = Header.Split(':', 2);
                            DictionaryHeader.Add(KeyAndValue[0], KeyAndValue[1]);
                        }

                        Signer.SetApiCustomHeaders(DictionaryHeader);
                    }
                }

                var Timestamp = new EdgeGridTimestamp();

                #region TemporaryPatchCode

                /*
                 * ***** IMPORTANT ******
                 *
                 * This part is usually not required, the signer library automatically gets the latest datetime from your system in UTC format
                 * but sometimes the date of your NTP may have an offset higher thant 30 seconds and that's when you will probably be hit with the infamous:
                 *  --> 400 Bad Request: Invalid Timestamp
                 * This happens because the timestamp of your local machine differs from the AKAMAI NTP server by more than 30 seconds 
                 * (30 seconds is the MAX amount of time a request can differ from the AKAMAI API NTP in order to be accepted)
                 * In order to bypass this problem I had to add (or substract) the seconds to make the request valid for AKAMAI
                 * You can check your time offeset by doing "ntpdate" on an unix console
                 */
                var TimestampInterval = new TimeSpan(0, 0, -15);
                Timestamp.SetValidFor(TimestampInterval);
                //Remove this lines if you don't need it

                #endregion

                Signer.Timestamp = Timestamp;

                HttpRequestMessage Request;
                try
                {
                    Request = Signer.GetRequestMessage();
                }
                catch (System.Exception Ex)
                {
                    throw new OpenApiException(Ex.Message);
                }

                if (Verbose)
                {
                    Console.WriteLine("Authorization: {0}", Request.Headers.Authorization);
                    Console.WriteLine();
                }

                var Response = Client.SendAsync(Request);
                Response.Wait();


                var ResponseContent = Response.Result.Content.ReadAsStringAsync();
                ResponseContent.Wait();

                string Result = ResponseContent.Result;

                if (string.IsNullOrWhiteSpace(OutputFile))
                {
                    Console.WriteLine("Request response: {0}", Result);
                }
                else
                {
                    try
                    {
                        System.IO.File.WriteAllText(OutputFile, Result);
                    }
                    catch (System.Exception Ex)
                    {
                        throw new OpenApiException(Ex.Message);
                    }
                }
            }
            catch (System.Exception Ex)
            {
                Console.WriteLine("OpenApi error. Type {0} with message {1}", Ex.GetType(), Ex.Message);
                if (Verbose)
                {
                    Console.WriteLine(Ex.StackTrace);
                }

                if (Ex.GetType() == typeof(OpenApiArgumentException) ||
                    Ex.GetType() == typeof(OpenApiCredentialException))
                {
                    Help();
                }
            }
        }

        public static void Help()
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