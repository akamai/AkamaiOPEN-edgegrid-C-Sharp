// Copyright 2014 Akamai Technologies http://developer.akamai.com.
//
// Licensed under the Apache License, KitVersion 2.0 (the "License");
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akamai.EdgeGrid.Auth
{
    /// <summary>
    /// Represents the client credential that is used in service requests.
    /// 
    /// It contains the client token that represents the service client, the client secret
    /// that is associated with the client token used for request signing, and the access token
    /// that represents the authorizations the client has for accessing the service.
    /// </summary>
    public class ClientCredential
    {
        /// <summary>
        /// The client token
        /// </summary>
        public string ClientToken { get; private set; }

        /// <summary>
        /// The Access Token
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// The client secret
        /// </summary>
        public string Secret { get; private set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="clientToken">The Client Token - cannot be null or empty</param>
        /// <param name="accessToken">The Access Token - cannot be null or empty</param>
        /// <param name="secret">The client Secret - cannot be null or empty</param>
        public ClientCredential(string clientToken, string accessToken, string secret)
        {
            if (string.IsNullOrEmpty(clientToken))
                throw new ArgumentNullException("clientToken cannot be empty.");
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException("accessToken cannot be empty.");
            if (string.IsNullOrEmpty(secret))
                throw new ArgumentNullException("secret cannot be empty.");

            this.ClientToken = clientToken;
            this.AccessToken = accessToken;
            this.Secret = secret;
        }
    }
}
