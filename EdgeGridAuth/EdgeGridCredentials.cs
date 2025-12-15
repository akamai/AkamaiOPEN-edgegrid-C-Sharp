using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Akamai.EdgeGrid.Auth
{
    /// <summary>
    /// Represents the client credential that is used in service requests.
    /// 
    /// It contains the client token that represents the service client, the client secret
    /// that is associated with the client token used for request signing, and the access token
    /// that represents the authorizations the client has for accessing the service.
    /// </summary>
    public class EdgeGridCredentials
    {
        /// <summary>
        /// Default maximum body size for POST request hashing (128 KB).
        /// </summary>
        public const int DefaultMaxBody = 131072;

        internal string? EdgeRCFile { get; set; }
        internal string? Section { get; set; }

        /// <summary>
        /// The host to connect to
        /// </summary>
        public string? Host { get; private set; } = "";

        /// <summary>
        /// The client token
        /// </summary>
        public string? ClientToken { get; private set; } = "";

        /// <summary>
        /// The client secret
        /// </summary>
        public string? ClientSecret { get; private set; } = "";

        /// <summary>
        /// The access token
        /// </summary>
        public string? AccessToken { get; private set; } = "";

        /// <summary>
        /// List of headers to include in the signature
        /// </summary>
        public List<string> HeadersToSign { get; private set; } = [];

        /// <summary>
        /// Maximum body size for POST request hashing
        /// </summary>
        public int MaxBody { get; private set; } = DefaultMaxBody;

        /// <summary>
        /// The account switch key for multi-account access
        /// </summary>
        public string? AccountKey { get; private set; }

        /// <summary>
        /// Constructor for direct credential assignment
        /// </summary>
        /// <param name="host">The host to connect to - cannot be null or empty</param>
        /// <param name="clientToken">The client token - cannot be null or empty</param>
        /// <param name="clientSecret">The client secret - cannot be null or empty</param>
        /// <param name="accessToken">The access token - cannot be null or empty</param>
        /// <param name="headersToSign">Optional list of headers to include in the signature</param>
        /// <param name="maxBody">Maximum body size for POST request hashing</param>
        /// <param name="accountKey">Optional account switch key for multi-account access</param>
        public EdgeGridCredentials(string host, string clientToken, string clientSecret, string accessToken,
            List<string>? headersToSign = null, int maxBody = DefaultMaxBody, string? accountKey = null)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            ClientToken = clientToken ?? throw new ArgumentNullException(nameof(clientToken));
            ClientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            HeadersToSign = headersToSign?.Select(h => h.ToLower()).ToList() ?? new List<string>();
            MaxBody = maxBody;
            AccountKey = accountKey;
        }

        /// <summary>
        /// Constructor that loads credentials from environment variables or EdgeRC file
        /// </summary>
        /// <param name="edgeRCFile">Path to the EdgeRC file (defaults to ~/.edgerc)</param>
        /// <param name="section">Section in the EdgeRC file to use (defaults to "default")</param>
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

            // Read from environment variables first, then fall back to file if needed
            if (edgeRCFile == null)
            {
                GetCredentialsFromEnvironment(Section);

                if (string.IsNullOrEmpty(Host) || string.IsNullOrEmpty(ClientToken) || 
                    string.IsNullOrEmpty(ClientSecret) || string.IsNullOrEmpty(AccessToken))
                {
                    // If any of the necessary elements are missing, try to read from the edgerc file
                    // This is useful for local development where environment variables may not be set
                    GetCredentialsFromEdgeRCFile(EdgeRCFile, Section);
                }
            }
            else
            {
                GetCredentialsFromEdgeRCFile(EdgeRCFile, Section);
            }

            if (string.IsNullOrEmpty(Host) || string.IsNullOrEmpty(ClientToken) || 
                string.IsNullOrEmpty(ClientSecret) || string.IsNullOrEmpty(AccessToken))
            {
                throw new InvalidOperationException("Failed to find credentials from environment variables or EdgeRCFile.");
            }
        }


        internal void GetCredentialsFromEnvironment(string section)
        {
            // For default section, use AKAMAI_HOST, AKAMAI_CLIENT_TOKEN, etc.
            // For other sections, use AKAMAI_{SECTION}_HOST, AKAMAI_{SECTION}_CLIENT_TOKEN, etc.
            string prefix = section.Equals("default", StringComparison.OrdinalIgnoreCase)
                ? "AKAMAI"
                : $"AKAMAI_{section.ToUpperInvariant()}";

            string AccessTokenVariable = $"{prefix}_ACCESS_TOKEN";
            string ClientTokenVariable = $"{prefix}_CLIENT_TOKEN";
            string ClientSecretVariable = $"{prefix}_CLIENT_SECRET";
            string HostVariable = $"{prefix}_HOST";
            string MaxBodyVariable = $"{prefix}_MAX_BODY";
            string AccountKeyVariable = $"{prefix}_ACCOUNT_KEY";

            this.AccessToken = Environment.GetEnvironmentVariable(AccessTokenVariable);
            this.ClientToken = Environment.GetEnvironmentVariable(ClientTokenVariable);
            this.ClientSecret = Environment.GetEnvironmentVariable(ClientSecretVariable);
            this.Host = Environment.GetEnvironmentVariable(HostVariable);
            this.AccountKey = Environment.GetEnvironmentVariable(AccountKeyVariable);

            string? maxBodyValue = Environment.GetEnvironmentVariable(MaxBodyVariable);
            if (!string.IsNullOrEmpty(maxBodyValue) && int.TryParse(maxBodyValue, out int maxBody))
            {
                this.MaxBody = maxBody;
            }
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
                                if (string.IsNullOrEmpty(this.Host))
                                    this.Host = value;
                                break;
                            case "client_token":
                                if (string.IsNullOrEmpty(this.ClientToken))
                                    this.ClientToken = value;
                                break;
                            case "client_secret":
                                if (string.IsNullOrEmpty(this.ClientSecret))
                                    this.ClientSecret = value;
                                break;
                            case "access_token":
                                if (string.IsNullOrEmpty(this.AccessToken))
                                    this.AccessToken = value;
                                break;
                            case "headers_to_sign":
                                if (this.HeadersToSign == null || this.HeadersToSign.Count == 0)
                                    this.HeadersToSign = value.Split(',').Select(h => h.Trim().ToLower()).ToList();
                                break;
                            case "max_body":
                                if (this.MaxBody == DefaultMaxBody && int.TryParse(value, out int maxBodyValue))
                                {
                                    this.MaxBody = maxBodyValue;
                                }
                                break;
                            case "account_key":
                                if (string.IsNullOrEmpty(this.AccountKey))
                                    this.AccountKey = value;
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
