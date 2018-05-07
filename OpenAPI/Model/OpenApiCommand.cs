using System;
using System.Collections.Generic;
using System.Linq;
using OpenApi.Exception;

namespace OpenApi.Model
{
    public class OpenApiCommand
    {
        public enum Argument
        {
            [OpenApiCommandStringValue("--a")] AccessToken,
            [OpenApiCommandStringValue("--c")] ClientToken,
            [OpenApiCommandStringValue("--d")] Data,
            [OpenApiCommandStringValue("--e")] EnvironmentCredential,
            [OpenApiCommandStringValue("--f")] SourceFile,
            [OpenApiCommandStringValue("--H")] HeaderLine,
            [OpenApiCommandStringValue("--o")] OutputFile,
            [OpenApiCommandStringValue("--r")] EdgercPath,
            [OpenApiCommandStringValue("--s")] Secret,
            [OpenApiCommandStringValue("--X")] Method,
            [OpenApiCommandStringValue("-h")] Help,
            [OpenApiCommandStringValue("--help")] Help1,
            [OpenApiCommandStringValue("-help")] Help2,
            [OpenApiCommandStringValue("/?")] Help3,
            [OpenApiCommandStringValue("-v")] Verbose,
            [OpenApiCommandStringValue("--v")] Verbose1,
            [OpenApiCommandStringValue("--u")] Url,
            [OpenApiCommandStringValue("")] Empty
        }

        public static Argument ValueFromString(string str)
        {
            switch (str)
            {
                case "--a": return Argument.AccessToken;
                case "--c": return Argument.ClientToken;
                case "--d": return Argument.Data;
                case "--e": return Argument.EnvironmentCredential;
                case "--f": return Argument.SourceFile;
                case "--H": return Argument.HeaderLine;
                case "--o": return Argument.OutputFile;
                case "--r": return Argument.EdgercPath;
                case "--s": return Argument.Secret;
                case "--X": return Argument.Method;
                case "-h": return Argument.Help;
                case "-help": return Argument.Help1;
                case "--help": return Argument.Help2;
                case "/?": return Argument.Help3;
                case "-v": return Argument.Verbose;
                case "--v": return Argument.Verbose1;
                default: return Argument.Empty;
            }
        }

        public static Dictionary<Argument, string> ExctratCommandArguments(string[] arguments)
        {
            var Result = new Dictionary<Argument, string>();
            var Url = arguments.Last();
            var IsValidUri = Uri.TryCreate(Url, UriKind.Absolute, out var UriResult) &&
                             (UriResult.Scheme == Uri.UriSchemeHttp || UriResult.Scheme == Uri.UriSchemeHttps);
            if (!IsValidUri)
            {
                throw new OpenApiArgumentException("Invalid Url");
            }

            Result.Add(Argument.Url, Url);
            arguments = arguments.Take(arguments.Length - 1).ToArray();

            for (int i = 0; i < arguments.Length; i++)
            {
                var CommandFromString = ValueFromString(arguments[i]);
                if (CommandFromString != Argument.Empty)
                {
                    if (
                        CommandFromString == Argument.Help || CommandFromString == Argument.Help1 ||
                        CommandFromString == Argument.Help2 || CommandFromString == Argument.Help3 ||
                        CommandFromString == Argument.Verbose || CommandFromString == Argument.Verbose1
                    )
                    {
                        Result.Add(CommandFromString, Boolean.TrueString);
                    }
                    else
                    {
                        foreach (Argument ArgumentItem in Enum.GetValues(typeof(Argument)))
                        {
                            if (CommandFromString == ArgumentItem)
                            {
                                if (!String.IsNullOrWhiteSpace(arguments[i + 1]))
                                {
                                    var ArgumentValue = arguments[i + 1];
                                    try
                                    {
                                        Result.Add(ArgumentItem, ArgumentValue);
                                    }
                                    catch (System.Exception)
                                    {
                                        throw new OpenApiArgumentException("The argument already exist");
                                    }

                                    i++;
                                    break;
                                }
                                else
                                {
                                    throw new OpenApiArgumentException("Invalid Argument Format");
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new OpenApiArgumentException("Invalid Argument Format");
                }
            }

            if ((!Result.ContainsKey(Argument.AccessToken) || !Result.ContainsKey(Argument.ClientToken) ||
                 !Result.ContainsKey(Argument.Secret)) && (!Result.ContainsKey(Argument.EnvironmentCredential)) &&
                (!Result.ContainsKey(Argument.EdgercPath)))
            {
                throw new OpenApiCredentialException("No credentials defined");
            }


            return Result;
        }
    }
}