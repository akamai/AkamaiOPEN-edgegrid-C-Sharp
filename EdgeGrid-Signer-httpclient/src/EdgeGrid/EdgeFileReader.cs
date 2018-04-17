using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Akamai.EdgeGrid
{
    internal static class EdgeFileReader
    {   
        internal static Credential CreateFromEdgeRcFile(string section, string path) {
            
            Credential fileCredential;

            if(File.Exists(path)) 
            {
                Dictionary<string,string> edgercDictionary = parseEdgeRcFile(section, path);
                fileCredential = new Credential(
                    edgercDictionary.ContainsKey("client_token")?  edgercDictionary["client_token"] : "",
                    edgercDictionary.ContainsKey("access_token")?  edgercDictionary["client_token"] : "",
                    edgercDictionary.ContainsKey("client_secret")?  edgercDictionary["client_token"] : "",
                    edgercDictionary.ContainsKey("host")?  edgercDictionary["client_token"] : "",
                    edgercDictionary.ContainsKey("maxSize")?  edgercDictionary["client_token"] : ""
                );
            }else{
                throw new Akamai.EdgeGrid.Exception.EdgeGridSignerException("edgerc file not found");
            }  
            return fileCredential;
        }  

        private static Dictionary<string,string> parseEdgeRcFile(string section, string path){
            
            Dictionary<string, string> dictionaryCredentials = new Dictionary<string, string>();
            
            string[] edgeRcFileArray = System.IO.File.ReadAllLines(path);
            string sectionToBeSearched = "[" + section + "]";
            if(edgeRcFileArray.Length > 0){
                for (int lineIndex=0; lineIndex < edgeRcFileArray.Length; ++lineIndex)
                    if (sectionToBeSearched.Equals(edgeRcFileArray[lineIndex].ToString().Trim())){
                        //if the section matches a section inside the file
                        //then load the properties    
                        dictionaryCredentials = iterateOverSectionCredentials (dictionaryCredentials, edgeRcFileArray,lineIndex+1);
                    }
            }

            return dictionaryCredentials; 
        }

        private static Dictionary<string, string> iterateOverSectionCredentials(Dictionary<string, string> dictionaryCredentials, string[] edgeRcFileArray, int startIndex){
            int maxRowsToRead;

            //we try to set the number of rows that should be read for section, the default number should be 5
            //if the section does not have 5 credentials and we are at the last section
            //we set the number of rows to be read to lesser number
            if (edgeRcFileArray.Length >= startIndex + 5){
                maxRowsToRead = 5; 
            } else {
                maxRowsToRead = edgeRcFileArray.Length - startIndex;
            }

            for(int currentIndex = startIndex; currentIndex <= startIndex + maxRowsToRead; ++currentIndex){
                addCredential(dictionaryCredentials, edgeRcFileArray[currentIndex]);
            }
            return dictionaryCredentials;
        }
        private static Dictionary<string, string> addCredential(Dictionary<string, string> dictionaryCredentials, string line){
            string[] propertiesToFind = new string[] { "client_secret", "host", "access_token", "client_token", "max_size" }; 
            string trimmedLine = line.Trim().Replace(" ","");
            string key = "";
            string value = "";
            string textToMatch = "";

            foreach(string property in propertiesToFind) {
                textToMatch = property + "=";
                if(trimmedLine.Contains(textToMatch)){
                    key = property;
                    value = trimmedLine.Split(textToMatch)[1];
                    if (!dictionaryCredentials.ContainsKey(key)){
                        dictionaryCredentials.Add(key,value);    
                    }else{
                        throw new Akamai.EdgeGrid.Exception.EdgeGridSignerException("Duplicate" + key +" found. Possible causes: the credential could be declared more than once or it's taking the credential from another section");
                    }
                }
            }
            return dictionaryCredentials;
        }

        private static void validateCredentialParams(Dictionary<string, string> dictionaryCredentials){
            
            string[] credentialsToValidate = new string[] { "client_secret", "host", "access_token", "client_token"}; 
            
            foreach( string credential in credentialsToValidate) {
                if (dictionaryCredentials.ContainsKey(credential)) {
                    if(string.IsNullOrWhiteSpace(dictionaryCredentials[credential])){
                        throw new Akamai.EdgeGrid.Exception.EdgeGridSignerException(credential + " should not be empty. Check if it's not empty inside the edgerc file");
                    }
                }else{
                        throw new Akamai.EdgeGrid.Exception.EdgeGridSignerException("Missing "+ credential + " could not be loaded from the edgerc file. Possible problems: syntax error, unexpected property inside the section or missing credential inside file");                    
                }
            }
        }
    }
}
