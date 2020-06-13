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
using System.IO;
using System.Net;

namespace Akamai.EdgeGrid.Auth
{
    public class WebRequestTestCreate : IWebRequestCreate
    {
        public WebRequestTestCreate()
        {
        }

        #region IWebRequestCreate Members

        public WebRequest Create(Uri uri)
        {
            return new HttpWebRequestTest(uri);
        }

        #endregion IWebRequestCreate Members
    }

    public class HttpWebRequestTest : WebRequest
    {
        public HttpStatusCode ResponseStatus { get; set; }
        public String ResponseStatusDescription { get; set; }
        public WebHeaderCollection ResponseHeaders { get; set; }
        public MemoryStream RequestStream { get; set; }

        public override string Method { get; set; }
        public override WebHeaderCollection Headers { get; set; }
        public override long ContentLength { get; set; }
        public override string ContentType { get; set; }

        public override Uri RequestUri { get { return this.itemUri; } }

        private Uri itemUri;
        public WebResponse NextResponse = null;

        public HttpWebRequestTest(Uri uri)
        {
            this.Method = "GET";
            this.Headers = new WebHeaderCollection { };
            this.ContentLength = -1;
            this.RequestStream = new MemoryStream();
            this.itemUri = uri;
        }

        public override Stream GetRequestStream()
        {
            return this.RequestStream;
        }

        public override WebResponse GetResponse()
        {
            return NextResponse;
        }

        public HttpWebResponseTest CreateResponse(HttpStatusCode responseStatus = HttpStatusCode.OK, String statusDescription = "OK", WebHeaderCollection responseHeaders = null)
        {
            return new HttpWebResponseTest(
                responseStatus,
                responseHeaders ?? new WebHeaderCollection { },
                this.itemUri,
                this.Method,
                statusDescription);
        }
    }
}