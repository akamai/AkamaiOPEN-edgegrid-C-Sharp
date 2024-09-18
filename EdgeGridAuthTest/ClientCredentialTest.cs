using System;
using Akamai.Utils;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akamai.EdgeGrid.Auth
{
    [TestClass]
    public class ClientCredentialTest
    {
        [TestMethod]
        public void ConstructorDefualtTest()
        {
            string accessToken = "access-token";
            string clientToken = "client-token";
            string secret = "secret";
            Assert.IsNotNull(new ClientCredential(clientToken, accessToken, secret));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorDefualtTest_NullClientToke()
        {
            string accessToken = "access-token";
            string secret = "secret";
            var credential = new ClientCredential(null, accessToken, secret);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorDefualtTest_NullAccessToken()
        {
            string clientToken = "client-token";
            string secret = "secret";
            var credential = new ClientCredential(clientToken, null, secret);
            
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorDefualtTest_NullSecret()
        {
            string accessToken = "access-token";
            string clientToken = "client-token";
            var credential = new ClientCredential(clientToken, accessToken, null); 
        }

        [TestMethod]
        public void GetterTest()
        {
            string accessToken = "access-token";
            string clientToken = "client-token";
            string secret = "secret";
            ClientCredential credential = new ClientCredential(clientToken, accessToken, secret);

            Assert.AreEqual(credential.AccessToken, accessToken);
            Assert.AreEqual(credential.ClientToken, clientToken);
            Assert.AreEqual(credential.Secret, secret);
        }

    }
}
