using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Akamai.EdgeGrid;
using Akamai.EdgeGrid.Exception; 

namespace Akamai.EdgeGrid
{
    public static class CredentialFactory
    {
        public static Credential CreateFromEnvironment() {
            string clientToken = "";
            string accessToken = "";
            string clientSecret = "";

            try {
                clientToken = Environment.GetEnvironmentVariable("AKAMAI_CLIENT_TOKEN");
                accessToken = Environment.GetEnvironmentVariable("AKAMAI_ACCESS_TOKEN");
                clientSecret = Environment.GetEnvironmentVariable("AKAMAI_CLIENT_SECRET");

            } catch(System.Exception SecurityException){
                Console.Write(SecurityException.Message);
            }

           return new Credential(clientToken, accessToken, clientSecret); 
        }

        public static Credential CreateFromEdgeRcFile(string section = "default", string path = ".edgerc") {
            
            string clientToken = "";
            string accessToken = "";
            string clientSecret = ""; 
            string host = "";

            if(File.Exists(path)) 
            {
                Dictionary<string,string> edgercDictionary = parseEdgeRcFile(section, path);
                clientToken =  edgercDictionary["client_secret"];
                accessToken = edgercDictionary["access_token"];
                clientSecret =  edgercDictionary["client_token"];

            }else{
                throw new System.IO.FileNotFoundException("edgerc file not found");
            }  
           return new Credential(clientToken, accessToken, clientSecret); 
        }  

        private static Dictionary<string,string> parseEdgeRcFile(string section, string path){
            
            Dictionary<string, string> parsedDicCredentials = new Dictionary<string, string>();
            
            string[] edgeRcFileArray = System.IO.File.ReadAllLines(path);
            string sectionToBeSearched = "[" + section + "]";
            int numberOfRemainingArgs = 4;
            bool isSectionFound = false;

            if(edgeRcFileArray.Length > 0){
                foreach(string line in edgeRcFileArray)  {
                    if(numberOfRemainingArgs > 0 && isSectionFound){
                       --numberOfRemainingArgs;
                       addUniqueCredentialProperty(ref parsedDicCredentials, line, numberOfRemainingArgs);
                    }
                    if (sectionToBeSearched.Equals(line.Trim()) && !isSectionFound){
                        isSectionFound = true;
                    }
                    if(numberOfRemainingArgs == 0){
                        break;
                    }

                }
            }

            validateCredentialParams(parsedDicCredentials);

            return parsedDicCredentials; 
        }

        private static void addUniqueCredentialProperty(ref Dictionary<string, string> parsedDicCredentials, string line, int numberOfRemainingArgs){
            
            string[] propertiesToFind = new string[] { "client_secret", "host", "access_token", "client_token" }; 
            string trimmedLine = line.Trim().Replace(" ","");
            string key = "";
            string value = "";
            string textToMatch = "";

            foreach(string property in propertiesToFind) {
                
                textToMatch = property + "=";
                if(trimmedLine.Contains(textToMatch)){
                    key = property;
                    value = trimmedLine.Split(textToMatch)[1];
                    if (!parsedDicCredentials.ContainsKey(key)){
                        parsedDicCredentials.Add(key,value);    
                    }else{
                        throw new System.Exception("Duplicate" + key +" found. Possible causes: the credential could be declared more than once or it's taking the credential from another section");
                    }
                }
            }
        }

        private static void validateCredentialParams (Dictionary<string, string> credentialProperty){
            
            string[] propertyToValidate = new string[] { "client_secret", "host", "access_token", "client_token" }; 
            
            foreach( string property in propertyToValidate) {
                if (credentialProperty.ContainsKey(property)) {
                    if(string.IsNullOrWhiteSpace(credentialProperty[property])){
                        //throw exception
                        throw new System.Exception("Not implemented yet");

                    }
                }else{
                        //throw exception
                    throw new CredentialPropertyNotFoundException("Missing "+ property + " could not be loaded from the edgerc file. Possible problems: syntax error, unexpected property inside the section or missing property inside file");
                }
            }
        }

    }
}