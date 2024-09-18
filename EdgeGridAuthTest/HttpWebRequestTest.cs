using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Akamai.EdgeGrid.Auth
{

    public class WebRequestTestCreate : IWebRequestCreate
    {
        public WebRequestTestCreate() { }

        #region IWebRequestCreate Members
        public WebRequest Create(Uri uri)
        {
            return new HttpWebRequestTest(uri);
        }
        #endregion
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

        public override Stream GetRequestStream() { return this.RequestStream; }

        public override WebResponse GetResponse()
        {
            return NextResponse;
        }

        public HttpWebResponseTest CreateResponse(HttpStatusCode responseStatus = HttpStatusCode.OK, String statusDescription = "OK", WebHeaderCollection responseHeaders = null)
        {
            SerializationInfo si = new SerializationInfo(typeof(HttpWebResponse), new System.Runtime.Serialization.FormatterConverter());
            StreamingContext sc = new StreamingContext();
            si.AddValue("m_HttpResponseHeaders", responseHeaders ?? new WebHeaderCollection { });
            si.AddValue("m_Uri", this.itemUri);
            si.AddValue("m_Certificate", null);
            si.AddValue("m_Version", HttpVersion.Version11);
            si.AddValue("m_StatusCode", responseStatus);
            si.AddValue("m_ContentLength", 0);
            si.AddValue("m_Verb", this.Method);
            si.AddValue("m_StatusDescription", statusDescription);
            si.AddValue("m_MediaType", null);
            HttpWebResponseTest response = new HttpWebResponseTest(si, sc);
            return response;
        }
        //TODO: Enable Async support for Tests
        //public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        //{
        //    Task<WebResponse> f = Task<WebResponse>.Factory.StartNew(
        //        _ =>
        //        {
        //            throw GetException();
        //        },
        //        state
        //    );
        //    if (callback != null) f.ContinueWith((res) => callback(f));
        //    return f;
        //}
        //public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        //{
        //    return ((Task<WebResponse>)asyncResult).Result;
        //}

    }

    public class HttpWebResponseTest : HttpWebResponse
    {
        public HttpWebResponseTest(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }

        public MemoryStream ResponseStream = new MemoryStream();
        public override Stream GetResponseStream()
        {
            return this.ResponseStream;
        }
    }
}
