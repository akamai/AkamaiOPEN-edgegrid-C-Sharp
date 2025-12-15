using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akamai.EdgeGrid;

namespace Akamai.EdgeGrid.ConsoleTest
{
    [TestClass]
    public class EdgeGridConsoleTest
    {
        [TestMethod]
        public void Test_ParseArguments_EmptyArgs()
        {
            string[] args = [];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.IsNotNull(options);
            Assert.AreEqual("", options.Path);
            Assert.AreEqual("GET", options.Method);
            Assert.AreEqual("application/json", options.ContentType);
            Assert.IsFalse(options.Verbose);
            Assert.IsFalse(options.ShowHelp);
        }

        [TestMethod]
        public void Test_ParseArguments_PathOnly()
        {
            string[] args = ["/test/path"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.AreEqual("/test/path", options.Path);
            Assert.AreEqual("GET", options.Method);
        }

        [TestMethod]
        public void Test_ParseArguments_PathWithFlag()
        {
            string[] args = ["-p", "/api/v1/test"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.AreEqual("/api/v1/test", options.Path);
            Assert.AreEqual("GET", options.Method);
        }

        [TestMethod]
        public void Test_ParseArguments_EdgeRCFile()
        {
            string[] args = ["-e", "/path/to/.edgerc", "-s", "section1"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.AreEqual("/path/to/.edgerc", options.EdgeRCFile);
            Assert.AreEqual("section1", options.Section);
        }

        [TestMethod]
        public void Test_ParseArguments_AccountSwitchKey()
        {
            string[] args = ["-a", "B-C-1234567", "/test/path"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.AreEqual("B-C-1234567", options.AccountSwitchKey);
            Assert.AreEqual("/test/path", options.Path);
        }

        [TestMethod]
        public void Test_ParseArguments_DataChangesMethodToPOST()
        {
            string[] args = ["-d", "{\"key\":\"value\"}", "/test/path"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.AreEqual("{\"key\":\"value\"}", options.Data);
            Assert.AreEqual("POST", options.Method);
            Assert.AreEqual("/test/path", options.Path);
        }

        [TestMethod]
        public void Test_ParseArguments_UploadFileChangesMethodToPUT()
        {
            string[] args = ["-f", "/path/to/file.json", "/test/path"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.AreEqual("/path/to/file.json", options.UploadFile);
            Assert.AreEqual("PUT", options.Method);
            Assert.AreEqual("/test/path", options.Path);
        }

        [TestMethod]
        public void Test_ParseArguments_ExplicitMethodOverridesDefault()
        {
            string[] args = ["-X", "DELETE", "/test/path"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.AreEqual("DELETE", options.Method);
            Assert.AreEqual("/test/path", options.Path);
        }

        [TestMethod]
        public void Test_ParseArguments_ExplicitMethodWithData()
        {
            string[] args = ["-X", "PATCH", "-d", "{\"data\":\"test\"}", "/test/path"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.AreEqual("PATCH", options.Method);
            Assert.AreEqual("{\"data\":\"test\"}", options.Data);
        }

        [TestMethod]
        public void Test_ParseArguments_Headers()
        {
            string[] args = ["-H", "X-Custom-Header: value1", "-H", "X-Another: value2", "/test/path"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.AreEqual(2, options.Headers.Count);
            Assert.AreEqual("X-Custom-Header: value1", options.Headers[0]);
            Assert.AreEqual("X-Another: value2", options.Headers[1]);
        }

        [TestMethod]
        public void Test_ParseArguments_OutputFile()
        {
            string[] args = ["-o", "output.json", "/test/path"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.AreEqual("output.json", options.OutputFile);
        }

        [TestMethod]
        public void Test_ParseArguments_ContentType()
        {
            string[] args = ["-T", "text/xml", "/test/path"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.AreEqual("text/xml", options.ContentType);
        }

        [TestMethod]
        public void Test_ParseArguments_Verbose()
        {
            string[] args = ["-v", "/test/path"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.IsTrue(options.Verbose);
        }

        [TestMethod]
        public void Test_ParseArguments_VerboseAlternate()
        {
            string[] args = ["-vv", "/test/path"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.IsTrue(options.Verbose);
        }

        [TestMethod]
        public void Test_ParseArguments_HelpShortFlag()
        {
            string[] args = ["-h"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.IsTrue(options.ShowHelp);
        }

        [TestMethod]
        public void Test_ParseArguments_HelpLongFlag()
        {
            string[] args = ["--help"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.IsTrue(options.ShowHelp);
        }

        [TestMethod]
        public void Test_ParseArguments_HelpSlashQuestion()
        {
            string[] args = ["/?"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.IsTrue(options.ShowHelp);
        }

        [TestMethod]
        public void Test_ParseArguments_ComplexScenario()
        {
            string[] args = [
                "-e", "/home/user/.edgerc",
                "-s", "production",
                "-a", "B-C-1234567",
                "-X", "POST",
                "-d", "{\"name\":\"test\"}",
                "-H", "X-Custom: value",
                "-T", "application/json",
                "-v",
                "/api/v1/resource"
            ];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.AreEqual("/home/user/.edgerc", options.EdgeRCFile);
            Assert.AreEqual("production", options.Section);
            Assert.AreEqual("B-C-1234567", options.AccountSwitchKey);
            Assert.AreEqual("POST", options.Method);
            Assert.AreEqual("{\"name\":\"test\"}", options.Data);
            Assert.AreEqual(1, options.Headers.Count);
            Assert.AreEqual("X-Custom: value", options.Headers[0]);
            Assert.AreEqual("application/json", options.ContentType);
            Assert.IsTrue(options.Verbose);
            Assert.AreEqual("/api/v1/resource", options.Path);
        }

        [TestMethod]
        public void Test_AddAccountSwitchKeyToPath_EmptyKey()
        {
            string path = "/test/path";
            string result = EdgeGridConsole.AddAccountSwitchKeyToPath(path, "");

            Assert.AreEqual("/test/path", result);
        }

        [TestMethod]
        public void Test_AddAccountSwitchKeyToPath_NullKey()
        {
            string path = "/test/path";
            string result = EdgeGridConsole.AddAccountSwitchKeyToPath(path, null!);

            Assert.AreEqual("/test/path", result);
        }

        [TestMethod]
        public void Test_AddAccountSwitchKeyToPath_PathWithoutQueryString()
        {
            string path = "/test/path";
            string result = EdgeGridConsole.AddAccountSwitchKeyToPath(path, "B-C-1234567");

            Assert.AreEqual("/test/path?accountSwitchKey=B-C-1234567", result);
        }

        [TestMethod]
        public void Test_AddAccountSwitchKeyToPath_PathWithQueryString()
        {
            string path = "/test/path?param1=value1";
            string result = EdgeGridConsole.AddAccountSwitchKeyToPath(path, "B-C-1234567");

            Assert.AreEqual("/test/path?param1=value1&accountSwitchKey=B-C-1234567", result);
        }

        [TestMethod]
        public void Test_AddAccountSwitchKeyToPath_PathWithMultipleQueryParams()
        {
            string path = "/test/path?param1=value1&param2=value2";
            string result = EdgeGridConsole.AddAccountSwitchKeyToPath(path, "B-C-1234567");

            Assert.AreEqual("/test/path?param1=value1&param2=value2&accountSwitchKey=B-C-1234567", result);
        }

        [TestMethod]
        public void Test_CommandLineOptions_DefaultValues()
        {
            var options = new CommandLineOptions();

            Assert.AreEqual("", options.EdgeRCFile);
            Assert.AreEqual("", options.Section);
            Assert.AreEqual("", options.AccountSwitchKey);
            Assert.AreEqual("", options.Path);
            Assert.IsNotNull(options.Headers);
            Assert.AreEqual(0, options.Headers.Count);
            Assert.AreEqual("GET", options.Method);
            Assert.AreEqual("application/json", options.ContentType);
            Assert.AreEqual("", options.OutputFile);
            Assert.AreEqual("", options.UploadFile);
            Assert.AreEqual("", options.Data);
            Assert.IsFalse(options.Verbose);
            Assert.IsFalse(options.ShowHelp);
        }

        [TestMethod]
        public void Test_ParseArguments_PathAsPositionalArgument()
        {
            // Path provided without flag should be treated as positional argument
            string[] args = ["-e", "/home/.edgerc", "/api/test"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.AreEqual("/home/.edgerc", options.EdgeRCFile);
            Assert.AreEqual("/api/test", options.Path);
        }

        [TestMethod]
        public void Test_ParseArguments_LastPathWins()
        {
            // If multiple paths provided, last one should win
            string[] args = ["/first/path", "-p", "/second/path", "/third/path"];
            var options = EdgeGridConsole.ParseArguments(args);

            Assert.AreEqual("/third/path", options.Path);
        }
    }
}
