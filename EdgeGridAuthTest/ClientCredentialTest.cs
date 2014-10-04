// Copyright 2014 Akamai Technologies http://developer.akamai.com.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Author: colinb@akamai.com  (Colin Bendell)
//

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
