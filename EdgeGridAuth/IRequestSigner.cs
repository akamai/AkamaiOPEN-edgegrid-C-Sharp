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
    /// </summary>
    interface IRequestSigner
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
