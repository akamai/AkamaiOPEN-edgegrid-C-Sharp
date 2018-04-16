using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Akamai.EdgeGrid
{
    public static class EnvironmentCredentialReader
    {
        public static Credential getCredential(string section){
                
            Credential environmentCredential;
            section = section.ToUpper();
            
            string sectionHost = Environment.GetEnvironmentVariable("AKAMAI_"+ section +"_HOST");
            string prefix = string.IsNullOrWhiteSpace(sectionHost) ? "AKAMAI_" : "AKAMAI_" +section +  "_";
            string ClientToken = Environment.GetEnvironmentVariable(prefix + "CLIENT_TOKEN");
            string AccessToken = Environment.GetEnvironmentVariable(prefix + "ACCESS_TOKEN");
            string ClientSecret = Environment.GetEnvironmentVariable(prefix + "CLIENT_SECRET");
            string bodySize = Environment.GetEnvironmentVariable(prefix + "MAX_SIZE");

            if (sectionHost == null){
                throw new System.Exception("host could not be loaded from environment");
            }
            if (ClientToken == null){
                throw new System.Exception("client_token could not be loaded from environment");
            }
            if (AccessToken == null){
                throw new System.Exception("access_token could not be loaded from environment");      
            }
            if (ClientSecret == null){
                throw new System.Exception("client_secret could not be loaded from environment");               
            }
            if (bodySize == null){
                //set the default body max size to 131072
                bodySize = "131072";              
            }
            environmentCredential = new Credential(ClientToken, AccessToken, ClientSecret, sectionHost, bodySize);
            return environmentCredential;
        }

    }
}