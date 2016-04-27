using Akamai.EdgeGrid.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Akamai.EdgeGrid.AuthTest
{
    [TestClass]
    public class HttpClientTests
    {
        [TestMethod]
        public void MyTestMethod()
        {
            var baseUrl = "https://<your base url prefix>.luna.akamaiapis.net";
            string clientToken = "client-token-abc";
            string accessToken = "access-token-def";
            string secret = "secret-shh";

            var creds = new ClientCredential(clientToken, accessToken, secret);
            var handler = new EdgeGridV1HttpMessageHandler(creds);

            using (var client = new HttpClient(handler))
            {
                var response = client.GetAsync($"{baseUrl}/diagnostic-tools/v1/locations").Result;

                Assert.IsTrue(response.IsSuccessStatusCode, response.ReasonPhrase);
            }
        }
    }
}
