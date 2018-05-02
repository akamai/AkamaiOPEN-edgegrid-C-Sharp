using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Akamai.EdgeGrid.Exception;

namespace Akamai.EdgeGrid
{
    internal static class EnvironmentCredentialReader
    {
        public static Credential GetCredential(string section)
        {
            Credential environmentCredential;
            section = section.ToUpper();

            string SectionHost = Environment.GetEnvironmentVariable("AKAMAI_" + section + "_HOST");
            string Prefix = string.IsNullOrWhiteSpace(SectionHost) ? "AKAMAI_" : "AKAMAI_" + section + "_";
            string ClientToken = Environment.GetEnvironmentVariable(Prefix + "CLIENT_TOKEN");
            string AccessToken = Environment.GetEnvironmentVariable(Prefix + "ACCESS_TOKEN");
            string ClientSecret = Environment.GetEnvironmentVariable(Prefix + "CLIENT_SECRET");
            string BodySize = Environment.GetEnvironmentVariable(Prefix + "MAX_SIZE");

            if (SectionHost == null)
            {
                throw new EdgeGridSignerException("host could not be loaded from environment");
            }
            if (ClientToken == null)
            {
                throw new EdgeGridSignerException("client token could not be loaded from environment");
            }
            if (AccessToken == null)
            {
                throw new EdgeGridSignerException("access token could not be loaded from environment");
            }
            if (ClientSecret == null)
            {
                throw new EdgeGridSignerException("client secret could not be loaded from environment");
            }
            if (BodySize == null)
            {
                BodySize = "";
            }
            environmentCredential = new Credential(ClientToken, AccessToken, ClientSecret, SectionHost, BodySize);
            return environmentCredential;
        }

    }
}