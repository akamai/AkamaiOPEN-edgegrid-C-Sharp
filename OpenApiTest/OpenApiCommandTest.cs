using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using OpenApi.Exception;
using OpenApi.Model;

namespace OpenApiTest
{
    [TestFixture]
    public class OpenApiCommandTest
    {
        [Test]
        public void GetArgumentString()
        {
            OpenApiCommand.Argument AccessTokenArgument = OpenApiCommand.Argument.AccessToken;
            OpenApiCommand.Argument DataArgument = OpenApiCommand.Argument.Data;
            OpenApiCommand.Argument EnvironmentCredentialArgument = OpenApiCommand.Argument.EnvironmentCredential;
            OpenApiCommand.Argument SourceFileArgument = OpenApiCommand.Argument.SourceFile;

            Assert.AreEqual(AccessTokenArgument.ValueToString(), "--a");
            Assert.AreEqual(DataArgument.ValueToString(), "--d");
            Assert.AreEqual(EnvironmentCredentialArgument.ValueToString(), "--e");
            Assert.AreEqual(SourceFileArgument.ValueToString(), "--f");
        }

        [Test]
        public void GetArgumentFromString()
        {
            Assert.AreEqual(OpenApiCommand.ValueFromString("--a"), OpenApiCommand.Argument.AccessToken);
            Assert.AreEqual(OpenApiCommand.ValueFromString("--d"), OpenApiCommand.Argument.Data);
            Assert.AreEqual(OpenApiCommand.ValueFromString("--e"), OpenApiCommand.Argument.EnvironmentCredential);
            Assert.AreEqual(OpenApiCommand.ValueFromString("--f"), OpenApiCommand.Argument.SourceFile);
        }

        [Test]
        public void ExtractCommandArgumentCredential()
        {
            string[] StringArrayOfCommandsCredentialFile =
            {
                "--r", "~/ederc.credential",
                "https://AkamaiHost.net/diagnostic-tools/v2/ghost-locations/available"
            };

            var CommandResultCredentialFile =
                OpenApiCommand.ExctratCommandArguments(StringArrayOfCommandsCredentialFile);
            Assert.AreEqual(CommandResultCredentialFile.Count, 2);

            string[] StringArrayOfCommandsEnvironmetCredential =
            {
                "--e", "true",
                "https://AkamaiHost.net/diagnostic-tools/v2/ghost-locations/available"
            };

            var CommandResultEnvironmentCredential =
                OpenApiCommand.ExctratCommandArguments(StringArrayOfCommandsEnvironmetCredential);
            Assert.AreEqual(CommandResultEnvironmentCredential.Count, 2);
        }

        [Test]
        public void ExtractCommandArgumentFromArrayOfString()
        {
            string[] StringArrayOfCommands =
            {
                "--a", "Access_Token", "--c", "Client_Token", "--s", "Client_Secret",
                "https://AkamaiHost.net/diagnostic-tools/v2/ghost-locations/available"
            };

            var CommandResult = OpenApiCommand.ExctratCommandArguments(StringArrayOfCommands);
            Assert.AreEqual(CommandResult.Count, 4);
        }
        
        [Test]
        public void ExtractCommandArgumentForVerbose()
        {
            string[] StringArrayOfCommandWithVerbose =
            {
                "--a", "Access_Token", "--c", "Client_Token", "--s", "Client_Secret", "--v",
                "https://AkamaiHost.net/diagnostic-tools/v2/ghost-locations/available"
            };

            var CommandResultForVerbose = OpenApiCommand.ExctratCommandArguments(StringArrayOfCommandWithVerbose);
            Assert.AreEqual(CommandResultForVerbose.Count, 5);
        }

        [Test]
        public void ExtractCommandArgumentInvalid()
        {
            string[] StringArrayOfCommandsCredentialInvalidFile =
            {
                "--r", "",
                "https://AkamaiHost.net/diagnostic-tools/v2/ghost-locations/available"
            };

            Assert.Throws<OpenApiArgumentException>(
                () => OpenApiCommand.ExctratCommandArguments(StringArrayOfCommandsCredentialInvalidFile)
            );

            string[] StringArrayOfCommandsIncompleteCredential1 =
            {
                "--a", "Access_Token", "--c", "Client_Token",
                "https://AkamaiHost.net/diagnostic-tools/v2/ghost-locations/available"
            };

            Assert.Throws<OpenApiCredentialException>(
                () => OpenApiCommand.ExctratCommandArguments(StringArrayOfCommandsIncompleteCredential1)
            );

            string[] StringArrayOfCommandsIncompleteCredential2 =
            {
                "--a", "Access_Token", "--c", "Client_Token", "--s", "",
                "https://AkamaiHost.net/diagnostic-tools/v2/ghost-locations/available"
            };
            
            Assert.Throws<OpenApiArgumentException>(
                () => OpenApiCommand.ExctratCommandArguments(StringArrayOfCommandsIncompleteCredential2)
            );

            string[] StringArrayOfCommandsIncompleteCredential3 =
            {
                "--a", "Access_Token", "--c", "Client_Token", "--c", "Client_Token",
                "https://AkamaiHost.net/diagnostic-tools/v2/ghost-locations/available"
            };
            
            Assert.Throws<OpenApiArgumentException>(
                () => OpenApiCommand.ExctratCommandArguments(StringArrayOfCommandsIncompleteCredential3)
            );
            
            string[] StringArrayOfCommandsInvalidUrl =
            {
                "--a", "Access_Token", "--c", "Client_Token", "--s", "Client_Secret",
                "adadsdasdad"
            };

            Assert.Throws<OpenApiArgumentException>(
                () => OpenApiCommand.ExctratCommandArguments(StringArrayOfCommandsInvalidUrl)
            );

            string[] StringArrayOfCommandsNoUrl =
            {
                "--a", "Access_Token", "--c", "Client_Token", "--s", "Client_Secret"
            };

            Assert.Throws<OpenApiArgumentException>(
                () => OpenApiCommand.ExctratCommandArguments(StringArrayOfCommandsNoUrl)
            );

            string[] StringArrayOfCommandsIncorrectFormat =
            {
                "--a", "--c", "Client_Token", "--s", "Client_Secret"
            };

            Assert.Throws<OpenApiArgumentException>(
                () => OpenApiCommand.ExctratCommandArguments(StringArrayOfCommandsIncorrectFormat)
            );
        }

        [Test]
        public void ExtractCommandArgumentWithExtraParameters()
        {
            string[] StringArrayOfCommands =
            {
                "--a", "Access_Token", "--c", "Client_Token", "--s", "Client_Secret",
                "--d", "{\"endUserName\": \"name\", \"url\": \"www.test.com\"}", "--e", "true", "--f",
                "{workspace}/OpenApi/postBody",
                "https://AkamaiHost.net/diagnostic-tools/v2/ghost-locations/available"
            };

            var CommandResult = OpenApiCommand.ExctratCommandArguments(StringArrayOfCommands);
            Assert.AreEqual(CommandResult.Count, 7);
        }
    }
}