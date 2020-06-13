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
    public class HttpWebResponseTest : HttpWebResponse
    {
        public WebHeaderCollection _Headers;
        public string _Method;
        public Uri _ResponseUri;
        public HttpStatusCode _StatusCode;
        public string _StatusDescription;
        public MemoryStream ResponseStream = new MemoryStream();

        public HttpWebResponseTest(
            HttpStatusCode responseStatus,
            WebHeaderCollection responseHeaders,
            Uri uri,
            string method,
            string description)
        {
            _StatusCode = responseStatus;
            _Headers = responseHeaders;
            _ResponseUri = uri;
            _Method = method;
            _StatusDescription = description;
        }

        public override long ContentLength => 0;
        public override WebHeaderCollection Headers => _Headers;
        public override string Method => _Method;
        public override Uri ResponseUri => _ResponseUri;
        public override HttpStatusCode StatusCode => _StatusCode;
        public override string StatusDescription => _StatusDescription;

        public override Stream GetResponseStream()
        {
            return this.ResponseStream;
        }
    }
}