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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Akamai.EdgeGrid.Auth
{
    /// <summary>
    /// Interface describing a request signer that signs service requests.
    /// 
    /// Author: colinb@akamai.com  (Colin Bendell)
    /// </summary>
    public interface IRequestSigner
    {
        /// <summary>
        /// Signs a request with the client credential.
        /// </summary>
        /// <param name="request">the web request object to sign</param>
        /// <param name="credential">the credential used in the signing</param>
        /// <param name="uploadStream">the optional stream to upload</param>
        /// <returns>the web request with the added signature headers</returns>
        WebRequest Sign(WebRequest request, ClientCredential credential, Stream uploadStream = null);

        /// <summary>
        /// Signs and Executes a request with the client credential.
        /// </summary>
        /// <param name="request">the web request object to sign</param>
        /// <param name="credential">the credential used in the signing</param>
        /// <param name="uploadStream">the optional stream to upload</param>
        /// <returns>the stream from the response (may be empty)</returns>
        Stream Execute(WebRequest request, ClientCredential credential, Stream uploadStream = null);
    }
}
