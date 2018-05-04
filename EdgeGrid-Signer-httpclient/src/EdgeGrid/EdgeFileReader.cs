using System;
using System.IO;
using System.Collections.Generic;

namespace Akamai.EdgeGrid
{
    internal static class EdgeFileReader
    {
        internal static Credential CreateFromEdgeRcFile(string section, string path)
        {
            Credential FileCredential;

            if (File.Exists(path))
            {
                Dictionary<string, string> EdgercDictionary = ParseEdgeRcFile(section, path);
                FileCredential = new Credential(
                    EdgercDictionary.ContainsKey("client_token") ? EdgercDictionary["client_token"] : "",
                    EdgercDictionary.ContainsKey("access_token") ? EdgercDictionary["access_token"] : "",
                    EdgercDictionary.ContainsKey("client_secret") ? EdgercDictionary["client_secret"] : "",
                    EdgercDictionary.ContainsKey("host") ? EdgercDictionary["host"] : "",
                    EdgercDictionary.ContainsKey("maxSize") ? EdgercDictionary["maxSize"] : ""
                );
            }
            else
            {
                throw new Exception.EdgeGridSignerException(String.Format("edgerc file not found, into path {0}", path));
            }
            return FileCredential;
        }

        private static Dictionary<string, string> ParseEdgeRcFile(string section, string path)
        {
            Dictionary<string, string> DictionaryCredentials = new Dictionary<string, string>();

            string[] EdgeRcFileArray = System.IO.File.ReadAllLines(path);
            string SectionToBeSearched = "[" + section + "]";
            if (EdgeRcFileArray.Length > 0)
            {
                for (int LineIndex = 0; LineIndex < EdgeRcFileArray.Length; ++LineIndex)
                    if (SectionToBeSearched.Equals(EdgeRcFileArray[LineIndex].ToString().Trim()))
                    {
                        //if the section matches a section inside the file
                        //then load the properties    
                        DictionaryCredentials = IterateOverSectionCredentials(DictionaryCredentials, EdgeRcFileArray, LineIndex + 1);
                    }
            }

            return DictionaryCredentials;
        }

        private static Dictionary<string, string> IterateOverSectionCredentials(Dictionary<string, string> dictionaryCredentials, string[] edgeRcFileArray, int startIndex)
        {
            int MaxRowsToRead;

            //we try to set the number of rows that should be read for section, the default number should be 5
            //if the section does not have 5 credentials and we are at the last section
            //we set the number of rows to be read to lesser number
            if (edgeRcFileArray.Length >= startIndex + 5)
            {
                MaxRowsToRead = 5;
            }
            else
            {
                MaxRowsToRead = edgeRcFileArray.Length - startIndex;
            }

            for (int CurrentIndex = startIndex; CurrentIndex <= startIndex + MaxRowsToRead; ++CurrentIndex)
            {
                AddCredential(dictionaryCredentials, edgeRcFileArray[CurrentIndex]);
            }
            return dictionaryCredentials;
        }

        private static Dictionary<string, string> AddCredential(Dictionary<string, string> dictionaryCredentials, string line)
        {
            string[] PropertiesToFind = new string[] { "client_secret", "host", "access_token", "client_token", "max_size" };
            string TrimmedLine = line.Trim().Replace(" ", "");
            string Key = "";
            string Value = "";
            string TextToMatch = "";

            foreach (string Property in PropertiesToFind)
            {
                TextToMatch = Property + "=";
                if (TrimmedLine.Contains(TextToMatch))
                {
                    Key = Property;
                    Value = TrimmedLine.Split(TextToMatch)[1];
                    if (!dictionaryCredentials.ContainsKey(Key))
                    {
                        dictionaryCredentials.Add(Key, Value);
                    }
                    else
                    {
                        throw new Exception.EdgeGridSignerException("Duplicate" + Key + " found. Possible causes: the credential could be declared more than once or it's taking the credential from another section");
                    }
                }
            }
            return dictionaryCredentials;
        }

        private static void ValidateCredentialParams(Dictionary<string, string> dictionaryCredentials)
        {
            string[] CredentialsToValidate = new string[] { "client_secret", "host", "access_token", "client_token" };

            foreach (string Credential in CredentialsToValidate)
            {
                if (dictionaryCredentials.ContainsKey(Credential))
                {
                    if (string.IsNullOrWhiteSpace(dictionaryCredentials[Credential]))
                    {
                        throw new Exception.EdgeGridSignerException(Credential + " should not be empty. Check if it's not empty inside the edgerc file");
                    }
                }
                else
                {
                    throw new Exception.EdgeGridSignerException("Missing " + Credential + " could not be loaded from the edgerc file. Possible problems: syntax error, unexpected property inside the section or missing credential inside file");
                }
            }
        }
    }
}
