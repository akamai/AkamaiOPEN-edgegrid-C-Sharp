using System;
using System.Collections.Generic;
using Akamai.EdgeGrid;

namespace Tests.model
{
    public class EdgeGridSignerHeader
    {
        public Credential Auth { get; set; }
        public Uri Uri { get; set; }
        public List<string> HeadersToSign { get; set; }
        public List<Dictionary<string, string>> Headers { get; set; }
        public EdgeGridNonce Nonce { get; set; }
        public EdgeGridTimestamp Timestamp { get; set; }
        public long? MaxBody { get; set; }
        public EdgeGridSignerHeaderDataMethod.Method? Method { get; set; }
        public string Path { get; set; }
        public string Expected { get; set; }
        public Dictionary<string, string> Query { get; set; }
        public string Body { get; set; }
        public string Name { get; set; }

        public EdgeGridSignerHeader(Credential auth, Uri uri, List<Dictionary<string, string>> headers,
            List<string> headersToSign, EdgeGridNonce nonce,
            EdgeGridTimestamp timestamp, long? maxBody, EdgeGridSignerHeaderDataMethod.Method? method, string path, string expected,
            Dictionary<string, string> query, string body, string name)
        {
            Auth = auth;
            Uri = uri;
            Headers = headers;
            HeadersToSign = headersToSign;
            Nonce = nonce;
            Timestamp = timestamp;
            MaxBody = maxBody;
            Method = method;
            Path = path;
            Expected = expected;
            Query = query;
            Body = body;
            Name = name;
        }
    }
}