using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Akamai.EdgeGrid.Auth
{
    public class EdgeGridCredentials
    {
        /// <summary>
        /// Default maximum body size for POST request hashing (128 KB).
        /// </summary>
        public const int DefaultMaxBody = 131072;

        internal string? EdgeRCFile { get; set; }
        internal string? Section { get; set; }
        public string? Host { get; private set; } = "";
        public string? ClientToken { get; private set; } = "";
        public string? ClientSecret { get; private set; } = "";
        public string? AccessToken { get; private set; } = "";
        public List<string> HeadersToSign { get; private set; } = new List<string>();
        public int MaxBody { get; private set; } = DefaultMaxBody;

        // Constructor for direct credential assignment (useful for testing)
        public EdgeGridCredentials(string host, string clientToken, string clientSecret, string accessToken,
            List<string>? headersToSign = null, int maxBody = DefaultMaxBody)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            ClientToken = clientToken ?? throw new ArgumentNullException(nameof(clientToken));
            ClientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            HeadersToSign = headersToSign?.Select(h => h.ToLower()).ToList() ?? new List<string>();
            MaxBody = maxBody;
        }

        public EdgeGridCredentials(string? edgeRCFile = null, string? section = "default")
        {
            if (string.IsNullOrEmpty(edgeRCFile))
            {
                EdgeRCFile = "~/.edgerc";
            }
            else
            {
                EdgeRCFile = edgeRCFile;
            }

            if (section == null || section.Trim() == "")
            {
                Section = "default";
            }
            else
            {
                Section = section;
            }

            // Read from environment variables
            if (edgeRCFile == null)
            {
                GetCredentialsFromEnvironment(Section);

                if (Host == "" || ClientToken == "" || ClientSecret == "" || AccessToken == "")
                {
                    // If any of the necessary elements are missing, try to read from the edgerc file
                    // This is useful for local development where environment variables may not be set
                    GetCredentialsFromEdgeRCFile(EdgeRCFile, Section);
                }
                // If necessary elements are still missing, try from edgerc file using default location
                GetCredentialsFromEdgeRCFile(EdgeRCFile, Section);
            }
            else
            {
                GetCredentialsFromEdgeRCFile(EdgeRCFile, Section);
            }

            if (Host == "" || ClientToken == "" || ClientSecret == "" || AccessToken == "")
            {
                throw new InvalidOperationException("Failed to find credentials from environment variables or EdgeRCFile.");
            }
        }


        internal void GetCredentialsFromEnvironment(string section)
        {
            string AccessTokenVariable;
            string ClientTokenVariable;
            string ClientSecretVariable;
            string HostVariable;
            string SectionPrefix = $"_{section.ToUpperInvariant()}";

            AccessTokenVariable = $"AKAMAI_{SectionPrefix}_ACCESS_TOKEN";
            ClientTokenVariable = $"AKAMAI_{SectionPrefix}_CLIENT_TOKEN";
            ClientSecretVariable = $"AKAMAI_{SectionPrefix}_CLIENT_SECRET";
            HostVariable = $"AKAMAI_{SectionPrefix}_HOST";

            this.AccessToken = Environment.GetEnvironmentVariable(AccessTokenVariable);
            this.ClientToken = Environment.GetEnvironmentVariable(ClientTokenVariable);
            this.ClientSecret = Environment.GetEnvironmentVariable(ClientSecretVariable);
            this.Host = Environment.GetEnvironmentVariable(HostVariable);
        }

        internal void GetCredentialsFromEdgeRCFile(string edgeRCFile, string? section = "default")
        {
            string ExpandedEdgeRCFile = ExpandUserPath(edgeRCFile);
            string EdgeRCContents = File.ReadAllText(ExpandedEdgeRCFile);
            String[] lines = EdgeRCContents.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith($"[{section}]"))
                {
                    // Found the section, now read the key-value pairs
                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        if (lines[j].StartsWith("["))
                        {
                            // Reached the next section, stop reading
                            break;
                        }
                        var keyValue = lines[j].Split('=', 2);
                        var key = keyValue[0].Trim();
                        var value = keyValue[1].Trim();
                        switch (key.ToLowerInvariant())
                        {
                            case "host":
                                this.Host = value;
                                break;
                            case "client_token":
                                this.ClientToken = value;
                                break;
                            case "client_secret":
                                this.ClientSecret = value;
                                break;
                            case "access_token":
                                this.AccessToken = value;
                                break;
                            case "headers_to_sign":
                                this.HeadersToSign = value.Split(',').Select(h => h.Trim().ToLower()).ToList();
                                break;
                            case "max_body":
                                if (int.TryParse(value, out int maxBodyValue))
                                {
                                    this.MaxBody = maxBodyValue;
                                }
                                break;
                        }
                    }
                    break; // Exit after processing the section
                }
            }
        }

        internal static string ExpandUserPath(string filePath)
        {
            if (filePath.StartsWith("~"))
            {
                string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(homeDirectory, filePath.Substring(2));
            }
            else
            {
                return filePath;
            }
        }
    }
}
