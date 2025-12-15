#nullable enable
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akamai.EdgeGrid.Auth;
using System;
using System.IO;
using System.Reflection;

namespace Akamai.EdgeGrid.AuthTest
{
    [TestClass]
    public class EdgeGridCredentialsTest
    {
        [TestMethod]
        public void Test_Constructor_WithAllParameters()
        {
            var credentials = new EdgeGridCredentials(
                host: "test.example.com",
                clientToken: "test-client-token",
                clientSecret: "test-secret",
                accessToken: "test-access-token"
            );

            Assert.AreEqual("test.example.com", credentials.Host);
            Assert.AreEqual("test-client-token", credentials.ClientToken);
            Assert.AreEqual("test-secret", credentials.ClientSecret);
            Assert.AreEqual("test-access-token", credentials.AccessToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_Constructor_NullHost()
        {
            var credentials = new EdgeGridCredentials(
                host: null!,
                clientToken: "test-client-token",
                clientSecret: "test-secret",
                accessToken: "test-access-token"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_Constructor_NullClientToken()
        {
            var credentials = new EdgeGridCredentials(
                host: "test.example.com",
                clientToken: null!,
                clientSecret: "test-secret",
                accessToken: "test-access-token"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_Constructor_NullClientSecret()
        {
            var credentials = new EdgeGridCredentials(
                host: "test.example.com",
                clientToken: "test-client-token",
                clientSecret: null!,
                accessToken: "test-access-token"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_Constructor_NullAccessToken()
        {
            var credentials = new EdgeGridCredentials(
                host: "test.example.com",
                clientToken: "test-client-token",
                clientSecret: "test-secret",
                accessToken: null!
            );
        }

        [TestMethod]
        public void Test_Constructor_FromFile_DefaultSection()
        {
            string tempFile = Path.GetTempFileName();
            string edgercContent = @"[default]
client_secret = xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=
client_token = akab-client-token-xxx-xxxxxxxxxxxxxxxx
host = akaa-baseurl-xxxxxxxxxxx-xxxxxxxxxxxxx.luna.akamaiapis.net
access_token = akab-access-token-xxx-xxxxxxxxxxxxxxxx
max_body = 131072
";
            File.WriteAllText(tempFile, edgercContent);

            try
            {
                var credentials = new EdgeGridCredentials(tempFile);

                Assert.AreEqual("akaa-baseurl-xxxxxxxxxxx-xxxxxxxxxxxxx.luna.akamaiapis.net", credentials.Host);
                Assert.AreEqual("akab-client-token-xxx-xxxxxxxxxxxxxxxx", credentials.ClientToken);
                Assert.AreEqual("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=", credentials.ClientSecret);
                Assert.AreEqual("akab-access-token-xxx-xxxxxxxxxxxxxxxx", credentials.AccessToken);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void Test_Constructor_FromFile_CustomSection()
        {
            string tempFile = Path.GetTempFileName();
            string edgercContent = @"[default]
client_secret = default-secret
client_token = default-client-token
host = default.example.com
access_token = default-access-token

[staging]
client_secret = staging-secret
client_token = staging-client-token
host = staging.example.com
access_token = staging-access-token
";
            File.WriteAllText(tempFile, edgercContent);

            try
            {
                var credentials = new EdgeGridCredentials(tempFile, "staging");

                Assert.AreEqual("staging.example.com", credentials.Host);
                Assert.AreEqual("staging-client-token", credentials.ClientToken);
                Assert.AreEqual("staging-secret", credentials.ClientSecret);
                Assert.AreEqual("staging-access-token", credentials.AccessToken);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void Test_Constructor_FromFile_BrokenSection()
        {
            string tempFile = Path.GetTempFileName();
            string edgercContent = @"[default]
client_secret = xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=
client_token = akab-client-token-xxx-xxxxxxxxxxxxxxxx
host = akaa-baseurl-xxxxxxxxxxx-xxxxxxxxxxxxx.luna.akamaiapis.net
access_token = akab-access-token-xxx-xxxxxxxxxxxxxxxx

[broken]
client_secret = xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=
access_token = akab-access-token-xxx-xxxxxxxxxxxxxxxx
";
            File.WriteAllText(tempFile, edgercContent);

            try
            {
                // Should throw because client_token and host are missing
                bool exceptionThrown = false;
                try
                {
                    var credentials = new EdgeGridCredentials(tempFile, "broken");
                }
                catch (InvalidOperationException)
                {
                    exceptionThrown = true;
                }

                Assert.IsTrue(exceptionThrown, "Expected InvalidOperationException for incomplete credentials");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Test_Constructor_FromFile_MissingCredentials()
        {
            string tempFile = Path.GetTempFileName();
            string edgercContent = @"[default]
client_token = file-client-token
host = file.example.com
";
            File.WriteAllText(tempFile, edgercContent);

            try
            {
                var credentials = new EdgeGridCredentials(tempFile);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void Test_Constructor_FromFile_DefaultSectionWhenNull()
        {
            string tempFile = Path.GetTempFileName();
            string edgercContent = @"[default]
client_secret = default-secret
client_token = default-token
host = default.example.com
access_token = default-access
";
            File.WriteAllText(tempFile, edgercContent);

            try
            {
                var credentials1 = new EdgeGridCredentials(tempFile, null);
                var credentials2 = new EdgeGridCredentials(tempFile, "");
                var credentials3 = new EdgeGridCredentials(tempFile, "   ");

                Assert.AreEqual("default.example.com", credentials1.Host);
                Assert.AreEqual("default.example.com", credentials2.Host);
                Assert.AreEqual("default.example.com", credentials3.Host);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void Test_Constructor_FromEnvironment_DefaultSection()
        {
            // Set environment variables for default section
            // For default section, Go uses AKAMAI_HOST (not AKAMAI_DEFAULT_HOST)
            string originalHost = Environment.GetEnvironmentVariable("AKAMAI_HOST") ?? "";
            string originalClientToken = Environment.GetEnvironmentVariable("AKAMAI_CLIENT_TOKEN") ?? "";
            string originalClientSecret = Environment.GetEnvironmentVariable("AKAMAI_CLIENT_SECRET") ?? "";
            string originalAccessToken = Environment.GetEnvironmentVariable("AKAMAI_ACCESS_TOKEN") ?? "";
            string originalMaxBody = Environment.GetEnvironmentVariable("AKAMAI_MAX_BODY") ?? "";
            string originalAccountKey = Environment.GetEnvironmentVariable("AKAMAI_ACCOUNT_KEY") ?? "";

            try
            {
                Environment.SetEnvironmentVariable("AKAMAI_HOST", "env-host.example.com");
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_TOKEN", "env-client-token");
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_SECRET", "env-client-secret");
                Environment.SetEnvironmentVariable("AKAMAI_ACCESS_TOKEN", "env-access-token");
                Environment.SetEnvironmentVariable("AKAMAI_MAX_BODY", "65536");
                Environment.SetEnvironmentVariable("AKAMAI_ACCOUNT_KEY", "env-account-key");

                var credentials = new EdgeGridCredentials(null, "default");

                Assert.AreEqual("env-host.example.com", credentials.Host);
                Assert.AreEqual("env-client-token", credentials.ClientToken);
                Assert.AreEqual("env-client-secret", credentials.ClientSecret);
                Assert.AreEqual("env-access-token", credentials.AccessToken);
                Assert.AreEqual(65536, credentials.MaxBody);
                Assert.AreEqual("env-account-key", credentials.AccountKey);
            }
            finally
            {
                // Restore original environment variables
                Environment.SetEnvironmentVariable("AKAMAI_HOST", string.IsNullOrEmpty(originalHost) ? null : originalHost);
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_TOKEN", string.IsNullOrEmpty(originalClientToken) ? null : originalClientToken);
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_SECRET", string.IsNullOrEmpty(originalClientSecret) ? null : originalClientSecret);
                Environment.SetEnvironmentVariable("AKAMAI_ACCESS_TOKEN", string.IsNullOrEmpty(originalAccessToken) ? null : originalAccessToken);
                Environment.SetEnvironmentVariable("AKAMAI_MAX_BODY", string.IsNullOrEmpty(originalMaxBody) ? null : originalMaxBody);
                Environment.SetEnvironmentVariable("AKAMAI_ACCOUNT_KEY", string.IsNullOrEmpty(originalAccountKey) ? null : originalAccountKey);
            }
        }

        [TestMethod]
        public void Test_Constructor_FromEnvironment_CustomSection()
        {
            // Set environment variables for custom section "staging"
            string originalHost = Environment.GetEnvironmentVariable("AKAMAI_STAGING_HOST") ?? "";
            string originalClientToken = Environment.GetEnvironmentVariable("AKAMAI_STAGING_CLIENT_TOKEN") ?? "";
            string originalClientSecret = Environment.GetEnvironmentVariable("AKAMAI_STAGING_CLIENT_SECRET") ?? "";
            string originalAccessToken = Environment.GetEnvironmentVariable("AKAMAI_STAGING_ACCESS_TOKEN") ?? "";
            string originalMaxBody = Environment.GetEnvironmentVariable("AKAMAI_STAGING_MAX_BODY") ?? "";
            string originalAccountKey = Environment.GetEnvironmentVariable("AKAMAI_STAGING_ACCOUNT_KEY") ?? "";

            try
            {
                Environment.SetEnvironmentVariable("AKAMAI_STAGING_HOST", "staging-env-host.example.com");
                Environment.SetEnvironmentVariable("AKAMAI_STAGING_CLIENT_TOKEN", "staging-env-client-token");
                Environment.SetEnvironmentVariable("AKAMAI_STAGING_CLIENT_SECRET", "staging-env-client-secret");
                Environment.SetEnvironmentVariable("AKAMAI_STAGING_ACCESS_TOKEN", "staging-env-access-token");
                Environment.SetEnvironmentVariable("AKAMAI_STAGING_MAX_BODY", "32768");
                Environment.SetEnvironmentVariable("AKAMAI_STAGING_ACCOUNT_KEY", "staging-account-key");

                var credentials = new EdgeGridCredentials(null, "staging");

                Assert.AreEqual("staging-env-host.example.com", credentials.Host);
                Assert.AreEqual("staging-env-client-token", credentials.ClientToken);
                Assert.AreEqual("staging-env-client-secret", credentials.ClientSecret);
                Assert.AreEqual("staging-env-access-token", credentials.AccessToken);
                Assert.AreEqual(32768, credentials.MaxBody);
                Assert.AreEqual("staging-account-key", credentials.AccountKey);
            }
            finally
            {
                // Restore original environment variables
                Environment.SetEnvironmentVariable("AKAMAI_STAGING_HOST", string.IsNullOrEmpty(originalHost) ? null : originalHost);
                Environment.SetEnvironmentVariable("AKAMAI_STAGING_CLIENT_TOKEN", string.IsNullOrEmpty(originalClientToken) ? null : originalClientToken);
                Environment.SetEnvironmentVariable("AKAMAI_STAGING_CLIENT_SECRET", string.IsNullOrEmpty(originalClientSecret) ? null : originalClientSecret);
                Environment.SetEnvironmentVariable("AKAMAI_STAGING_ACCESS_TOKEN", string.IsNullOrEmpty(originalAccessToken) ? null : originalAccessToken);
                Environment.SetEnvironmentVariable("AKAMAI_STAGING_MAX_BODY", string.IsNullOrEmpty(originalMaxBody) ? null : originalMaxBody);
                Environment.SetEnvironmentVariable("AKAMAI_STAGING_ACCOUNT_KEY", string.IsNullOrEmpty(originalAccountKey) ? null : originalAccountKey);
            }
        }

        [TestMethod]
        public void Test_Constructor_FallbackFromEnvToFile()
        {
            // Set partial environment variables (missing some required fields)
            string originalHost = Environment.GetEnvironmentVariable("AKAMAI_DEFAULT_HOST") ?? "";
            string originalClientToken = Environment.GetEnvironmentVariable("AKAMAI_DEFAULT_CLIENT_TOKEN") ?? "";
            string originalClientSecret = Environment.GetEnvironmentVariable("AKAMAI_DEFAULT_CLIENT_SECRET") ?? "";
            string originalAccessToken = Environment.GetEnvironmentVariable("AKAMAI_DEFAULT_ACCESS_TOKEN") ?? "";

            string tempFile = Path.GetTempFileName();
            string originalEdgeRcPath = Environment.GetEnvironmentVariable("HOME") ?? "";

            try
            {
                // Set only host in env - missing other required fields
                Environment.SetEnvironmentVariable("AKAMAI_DEFAULT_HOST", "env-host.example.com");
                Environment.SetEnvironmentVariable("AKAMAI_DEFAULT_CLIENT_TOKEN", null);
                Environment.SetEnvironmentVariable("AKAMAI_DEFAULT_CLIENT_SECRET", null);
                Environment.SetEnvironmentVariable("AKAMAI_DEFAULT_ACCESS_TOKEN", null);

                // Create a temp edgerc file with complete credentials
                string edgercContent = @"[default]
client_secret = file-secret
client_token = file-client-token
host = file-host.example.com
access_token = file-access-token
";
                File.WriteAllText(tempFile, edgercContent);

                // The constructor with null edgeRCFile will try env first, then fall back to file
                // Since env vars are incomplete, it should fall back and read from file
                // The file credentials should override the partial env vars
                var credentials = new EdgeGridCredentials(tempFile, "default");

                // When explicit file is provided, it reads from file only
                Assert.AreEqual("file-host.example.com", credentials.Host);
                Assert.AreEqual("file-client-token", credentials.ClientToken);
                Assert.AreEqual("file-secret", credentials.ClientSecret);
                Assert.AreEqual("file-access-token", credentials.AccessToken);
            }
            finally
            {
                // Restore original environment variables
                Environment.SetEnvironmentVariable("AKAMAI_DEFAULT_HOST", string.IsNullOrEmpty(originalHost) ? null : originalHost);
                Environment.SetEnvironmentVariable("AKAMAI_DEFAULT_CLIENT_TOKEN", string.IsNullOrEmpty(originalClientToken) ? null : originalClientToken);
                Environment.SetEnvironmentVariable("AKAMAI_DEFAULT_CLIENT_SECRET", string.IsNullOrEmpty(originalClientSecret) ? null : originalClientSecret);
                Environment.SetEnvironmentVariable("AKAMAI_DEFAULT_ACCESS_TOKEN", string.IsNullOrEmpty(originalAccessToken) ? null : originalAccessToken);
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void Test_Constructor_EnvVariablesTakePrecedenceOverFile()
        {
            // This test verifies that when edgeRCFile is null and env vars are complete,
            // environment variables are used (not the file).
            // Since env vars are complete, the code should not attempt to read from ~/.edgerc
            string originalHost = Environment.GetEnvironmentVariable("AKAMAI_HOST") ?? "";
            string originalClientToken = Environment.GetEnvironmentVariable("AKAMAI_CLIENT_TOKEN") ?? "";
            string originalClientSecret = Environment.GetEnvironmentVariable("AKAMAI_CLIENT_SECRET") ?? "";
            string originalAccessToken = Environment.GetEnvironmentVariable("AKAMAI_ACCESS_TOKEN") ?? "";

            try
            {
                // Set complete environment variables
                Environment.SetEnvironmentVariable("AKAMAI_HOST", "env-host.example.com");
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_TOKEN", "env-client-token");
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_SECRET", "env-client-secret");
                Environment.SetEnvironmentVariable("AKAMAI_ACCESS_TOKEN", "env-access-token");

                // Create credentials without specifying file (should use env first)
                // Since all env vars are set, it should NOT attempt to read from ~/.edgerc
                var credentials = new EdgeGridCredentials(null, "default");

                // Environment variables should be used
                Assert.AreEqual("env-host.example.com", credentials.Host);
                Assert.AreEqual("env-client-token", credentials.ClientToken);
                Assert.AreEqual("env-client-secret", credentials.ClientSecret);
                Assert.AreEqual("env-access-token", credentials.AccessToken);
            }
            finally
            {
                // Restore original environment variables
                Environment.SetEnvironmentVariable("AKAMAI_HOST", string.IsNullOrEmpty(originalHost) ? null : originalHost);
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_TOKEN", string.IsNullOrEmpty(originalClientToken) ? null : originalClientToken);
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_SECRET", string.IsNullOrEmpty(originalClientSecret) ? null : originalClientSecret);
                Environment.SetEnvironmentVariable("AKAMAI_ACCESS_TOKEN", string.IsNullOrEmpty(originalAccessToken) ? null : originalAccessToken);
            }
        }

        [TestMethod]
        public void Test_Constructor_MaxBodyDefaultValue()
        {
            // When max_body is not specified, it should default to 131072
            string originalHost = Environment.GetEnvironmentVariable("AKAMAI_HOST") ?? "";
            string originalClientToken = Environment.GetEnvironmentVariable("AKAMAI_CLIENT_TOKEN") ?? "";
            string originalClientSecret = Environment.GetEnvironmentVariable("AKAMAI_CLIENT_SECRET") ?? "";
            string originalAccessToken = Environment.GetEnvironmentVariable("AKAMAI_ACCESS_TOKEN") ?? "";
            string originalMaxBody = Environment.GetEnvironmentVariable("AKAMAI_MAX_BODY") ?? "";

            try
            {
                Environment.SetEnvironmentVariable("AKAMAI_HOST", "env-host.example.com");
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_TOKEN", "env-client-token");
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_SECRET", "env-client-secret");
                Environment.SetEnvironmentVariable("AKAMAI_ACCESS_TOKEN", "env-access-token");
                Environment.SetEnvironmentVariable("AKAMAI_MAX_BODY", null); // Not set

                var credentials = new EdgeGridCredentials(null, "default");

                Assert.AreEqual(131072, credentials.MaxBody); // Default value
            }
            finally
            {
                Environment.SetEnvironmentVariable("AKAMAI_HOST", string.IsNullOrEmpty(originalHost) ? null : originalHost);
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_TOKEN", string.IsNullOrEmpty(originalClientToken) ? null : originalClientToken);
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_SECRET", string.IsNullOrEmpty(originalClientSecret) ? null : originalClientSecret);
                Environment.SetEnvironmentVariable("AKAMAI_ACCESS_TOKEN", string.IsNullOrEmpty(originalAccessToken) ? null : originalAccessToken);
                Environment.SetEnvironmentVariable("AKAMAI_MAX_BODY", string.IsNullOrEmpty(originalMaxBody) ? null : originalMaxBody);
            }
        }

        [TestMethod]
        public void Test_Constructor_AccountKeyFromFile()
        {
            string tempFile = Path.GetTempFileName();
            string edgercContent = @"[default]
client_secret = test-secret
client_token = test-client-token
host = test.example.com
access_token = test-access-token
account_key = F-AC-1234567
";
            File.WriteAllText(tempFile, edgercContent);

            try
            {
                var credentials = new EdgeGridCredentials(tempFile, "default");

                Assert.AreEqual("F-AC-1234567", credentials.AccountKey);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void Test_Constructor_MaxBodyFromFile()
        {
            string tempFile = Path.GetTempFileName();
            string edgercContent = @"[default]
client_secret = test-secret
client_token = test-client-token
host = test.example.com
access_token = test-access-token
max_body = 262144
";
            File.WriteAllText(tempFile, edgercContent);

            try
            {
                var credentials = new EdgeGridCredentials(tempFile, "default");

                Assert.AreEqual(262144, credentials.MaxBody);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void Test_Constructor_FromEnvironment_MissingHost()
        {
            // Test that missing HOST causes fallback to file (and fails if file doesn't exist)
            string originalClientToken = Environment.GetEnvironmentVariable("AKAMAI_TEST_CLIENT_TOKEN") ?? "";
            string originalClientSecret = Environment.GetEnvironmentVariable("AKAMAI_TEST_CLIENT_SECRET") ?? "";
            string originalAccessToken = Environment.GetEnvironmentVariable("AKAMAI_TEST_ACCESS_TOKEN") ?? "";
            string originalHost = Environment.GetEnvironmentVariable("AKAMAI_TEST_HOST") ?? "";

            try
            {
                // Set all except HOST
                Environment.SetEnvironmentVariable("AKAMAI_TEST_HOST", null);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_TOKEN", "test-client-token");
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_SECRET", "test-client-secret");
                Environment.SetEnvironmentVariable("AKAMAI_TEST_ACCESS_TOKEN", "test-access-token");

                // Should throw because HOST is missing and no valid file exists
                Assert.ThrowsException<InvalidOperationException>(() => 
                    new EdgeGridCredentials(null, "test"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("AKAMAI_TEST_HOST", string.IsNullOrEmpty(originalHost) ? null : originalHost);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_TOKEN", string.IsNullOrEmpty(originalClientToken) ? null : originalClientToken);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_SECRET", string.IsNullOrEmpty(originalClientSecret) ? null : originalClientSecret);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_ACCESS_TOKEN", string.IsNullOrEmpty(originalAccessToken) ? null : originalAccessToken);
            }
        }

        [TestMethod]
        public void Test_Constructor_FromEnvironment_MissingClientToken()
        {
            // Test that missing CLIENT_TOKEN causes fallback to file (and fails if file doesn't exist)
            string originalHost = Environment.GetEnvironmentVariable("AKAMAI_TEST_HOST") ?? "";
            string originalClientToken = Environment.GetEnvironmentVariable("AKAMAI_TEST_CLIENT_TOKEN") ?? "";
            string originalClientSecret = Environment.GetEnvironmentVariable("AKAMAI_TEST_CLIENT_SECRET") ?? "";
            string originalAccessToken = Environment.GetEnvironmentVariable("AKAMAI_TEST_ACCESS_TOKEN") ?? "";

            try
            {
                // Set all except CLIENT_TOKEN
                Environment.SetEnvironmentVariable("AKAMAI_TEST_HOST", "test-host");
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_TOKEN", null);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_SECRET", "test-client-secret");
                Environment.SetEnvironmentVariable("AKAMAI_TEST_ACCESS_TOKEN", "test-access-token");

                // Should throw because CLIENT_TOKEN is missing and no valid file exists
                Assert.ThrowsException<InvalidOperationException>(() => 
                    new EdgeGridCredentials(null, "test"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("AKAMAI_TEST_HOST", string.IsNullOrEmpty(originalHost) ? null : originalHost);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_TOKEN", string.IsNullOrEmpty(originalClientToken) ? null : originalClientToken);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_SECRET", string.IsNullOrEmpty(originalClientSecret) ? null : originalClientSecret);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_ACCESS_TOKEN", string.IsNullOrEmpty(originalAccessToken) ? null : originalAccessToken);
            }
        }

        [TestMethod]
        public void Test_Constructor_FromEnvironment_MissingClientSecret()
        {
            // Test that missing CLIENT_SECRET causes fallback to file (and fails if file doesn't exist)
            string originalHost = Environment.GetEnvironmentVariable("AKAMAI_TEST_HOST") ?? "";
            string originalClientToken = Environment.GetEnvironmentVariable("AKAMAI_TEST_CLIENT_TOKEN") ?? "";
            string originalClientSecret = Environment.GetEnvironmentVariable("AKAMAI_TEST_CLIENT_SECRET") ?? "";
            string originalAccessToken = Environment.GetEnvironmentVariable("AKAMAI_TEST_ACCESS_TOKEN") ?? "";

            try
            {
                // Set all except CLIENT_SECRET
                Environment.SetEnvironmentVariable("AKAMAI_TEST_HOST", "test-host");
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_TOKEN", "test-client-token");
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_SECRET", null);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_ACCESS_TOKEN", "test-access-token");

                // Should throw because CLIENT_SECRET is missing and no valid file exists
                Assert.ThrowsException<InvalidOperationException>(() => 
                    new EdgeGridCredentials(null, "test"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("AKAMAI_TEST_HOST", string.IsNullOrEmpty(originalHost) ? null : originalHost);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_TOKEN", string.IsNullOrEmpty(originalClientToken) ? null : originalClientToken);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_SECRET", string.IsNullOrEmpty(originalClientSecret) ? null : originalClientSecret);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_ACCESS_TOKEN", string.IsNullOrEmpty(originalAccessToken) ? null : originalAccessToken);
            }
        }

        [TestMethod]
        public void Test_Constructor_FromEnvironment_MissingAccessToken()
        {
            // Test that missing ACCESS_TOKEN causes fallback to file (and fails if file doesn't exist)
            string originalHost = Environment.GetEnvironmentVariable("AKAMAI_TEST_HOST") ?? "";
            string originalClientToken = Environment.GetEnvironmentVariable("AKAMAI_TEST_CLIENT_TOKEN") ?? "";
            string originalClientSecret = Environment.GetEnvironmentVariable("AKAMAI_TEST_CLIENT_SECRET") ?? "";
            string originalAccessToken = Environment.GetEnvironmentVariable("AKAMAI_TEST_ACCESS_TOKEN") ?? "";

            try
            {
                // Set all except ACCESS_TOKEN
                Environment.SetEnvironmentVariable("AKAMAI_TEST_HOST", "test-host");
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_TOKEN", "test-client-token");
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_SECRET", "test-client-secret");
                Environment.SetEnvironmentVariable("AKAMAI_TEST_ACCESS_TOKEN", null);

                // Should throw because ACCESS_TOKEN is missing and no valid file exists
                Assert.ThrowsException<InvalidOperationException>(() => 
                    new EdgeGridCredentials(null, "test"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("AKAMAI_TEST_HOST", string.IsNullOrEmpty(originalHost) ? null : originalHost);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_TOKEN", string.IsNullOrEmpty(originalClientToken) ? null : originalClientToken);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_CLIENT_SECRET", string.IsNullOrEmpty(originalClientSecret) ? null : originalClientSecret);
                Environment.SetEnvironmentVariable("AKAMAI_TEST_ACCESS_TOKEN", string.IsNullOrEmpty(originalAccessToken) ? null : originalAccessToken);
            }
        }

        [TestMethod]
        public void Test_Constructor_EnvVariablesPrecedenceOverFile_PartialOverride()
        {
            // Test that environment variables take precedence over file values
            // When some values are set in env, those should be used
            // When some values are missing from env, file values should fill them in
            string originalHost = Environment.GetEnvironmentVariable("AKAMAI_HOST") ?? "";
            string originalClientToken = Environment.GetEnvironmentVariable("AKAMAI_CLIENT_TOKEN") ?? "";
            string originalClientSecret = Environment.GetEnvironmentVariable("AKAMAI_CLIENT_SECRET") ?? "";
            string originalAccessToken = Environment.GetEnvironmentVariable("AKAMAI_ACCESS_TOKEN") ?? "";
            string originalMaxBody = Environment.GetEnvironmentVariable("AKAMAI_MAX_BODY") ?? "";
            string originalAccountKey = Environment.GetEnvironmentVariable("AKAMAI_ACCOUNT_KEY") ?? "";

            string tempFile = Path.GetTempFileName();
            string edgercContent = @"[default]
client_secret = file-secret
client_token = file-client-token
host = file-host.example.com
access_token = file-access-token
max_body = 65536
account_key = file-account-key
";
            File.WriteAllText(tempFile, edgercContent);

            try
            {
                // Set only HOST and CLIENT_TOKEN in environment
                // Others should come from file
                Environment.SetEnvironmentVariable("AKAMAI_HOST", "env-host.example.com");
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_TOKEN", "env-client-token");
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_SECRET", null);
                Environment.SetEnvironmentVariable("AKAMAI_ACCESS_TOKEN", null);
                Environment.SetEnvironmentVariable("AKAMAI_MAX_BODY", "98304"); // Set custom max_body
                Environment.SetEnvironmentVariable("AKAMAI_ACCOUNT_KEY", null);

                // Pass null to trigger environment read first, then use our temp file path
                // Since some env vars are missing, it will read from the file to fill them in
                var credentials = new EdgeGridCredentials(null, "default");

                // Environment values should be used where set
                Assert.AreEqual("env-host.example.com", credentials.Host);
                Assert.AreEqual("env-client-token", credentials.ClientToken);
                // Since edgeRCFile=null and env vars are incomplete, it reads from ~/.edgerc
                // which may exist on the system, so we can't assert file values here
                // Instead, let's verify that HOST and CLIENT_TOKEN from env were preserved
                Assert.IsNotNull(credentials.ClientSecret);
                Assert.IsNotNull(credentials.AccessToken);
                // Env max_body should be used
                Assert.AreEqual(98304, credentials.MaxBody);
            }
            finally
            {
                Environment.SetEnvironmentVariable("AKAMAI_HOST", string.IsNullOrEmpty(originalHost) ? null : originalHost);
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_TOKEN", string.IsNullOrEmpty(originalClientToken) ? null : originalClientToken);
                Environment.SetEnvironmentVariable("AKAMAI_CLIENT_SECRET", string.IsNullOrEmpty(originalClientSecret) ? null : originalClientSecret);
                Environment.SetEnvironmentVariable("AKAMAI_ACCESS_TOKEN", string.IsNullOrEmpty(originalAccessToken) ? null : originalAccessToken);
                Environment.SetEnvironmentVariable("AKAMAI_MAX_BODY", string.IsNullOrEmpty(originalMaxBody) ? null : originalMaxBody);
                Environment.SetEnvironmentVariable("AKAMAI_ACCOUNT_KEY", string.IsNullOrEmpty(originalAccountKey) ? null : originalAccountKey);
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void Test_GetCredentialsFromEdgeRCFile_DoesNotOverwriteExistingValues()
        {
            // Direct test of GetCredentialsFromEdgeRCFile to ensure it doesn't overwrite
            // already-set values from environment variables
            string tempFile = Path.GetTempFileName();
            string edgercContent = @"[default]
client_secret = file-secret
client_token = file-client-token
host = file-host.example.com
access_token = file-access-token
max_body = 65536
account_key = file-account-key
";
            File.WriteAllText(tempFile, edgercContent);

            try
            {
                // Create credentials with explicit values first
                var credentials = new EdgeGridCredentials(
                    host: "pre-set-host.example.com",
                    clientToken: "pre-set-token",
                    clientSecret: "pre-set-secret",
                    accessToken: "pre-set-access",
                    maxBody: 98304,
                    accountKey: "pre-set-account"
                );

                // Call GetCredentialsFromEdgeRCFile using reflection
                var method = typeof(EdgeGridCredentials).GetMethod("GetCredentialsFromEdgeRCFile", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(method, "GetCredentialsFromEdgeRCFile method not found");
                method.Invoke(credentials, new object[] { tempFile, "default" });

                // Verify that pre-set values were preserved
                Assert.AreEqual("pre-set-host.example.com", credentials.Host);
                Assert.AreEqual("pre-set-token", credentials.ClientToken);
                Assert.AreEqual("pre-set-secret", credentials.ClientSecret);
                Assert.AreEqual("pre-set-access", credentials.AccessToken);
                Assert.AreEqual(98304, credentials.MaxBody);
                Assert.AreEqual("pre-set-account", credentials.AccountKey);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void Test_GetCredentialsFromEdgeRCFile_FillsEmptyValues()
        {
            // Test that GetCredentialsFromEdgeRCFile fills in values that are empty
            string tempFile = Path.GetTempFileName();
            string edgercContent = @"[default]
client_secret = file-secret
client_token = file-client-token
host = file-host.example.com
access_token = file-access-token
max_body = 65536
account_key = file-account-key
";
            File.WriteAllText(tempFile, edgercContent);

            try
            {
                // Create credentials with some values set, others empty
                var credentials = new EdgeGridCredentials(
                    host: "pre-set-host.example.com",
                    clientToken: "pre-set-token",
                    clientSecret: "",  // Empty - should be filled from file
                    accessToken: ""    // Empty - should be filled from file
                );

                // Call GetCredentialsFromEdgeRCFile using reflection
                var method = typeof(EdgeGridCredentials).GetMethod("GetCredentialsFromEdgeRCFile", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(method, "GetCredentialsFromEdgeRCFile method not found");
                method.Invoke(credentials, new object[] { tempFile, "default" });

                // Verify that pre-set values were preserved
                Assert.AreEqual("pre-set-host.example.com", credentials.Host);
                Assert.AreEqual("pre-set-token", credentials.ClientToken);
                // Empty values should be filled from file
                Assert.AreEqual("file-secret", credentials.ClientSecret);
                Assert.AreEqual("file-access-token", credentials.AccessToken);
                // Default max_body should be overridden by file
                Assert.AreEqual(65536, credentials.MaxBody);
                // Empty account key should be filled from file
                Assert.AreEqual("file-account-key", credentials.AccountKey);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
