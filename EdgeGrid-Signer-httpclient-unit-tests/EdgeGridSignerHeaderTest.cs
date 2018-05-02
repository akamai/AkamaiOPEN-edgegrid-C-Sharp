using System;
using System.Collections.Generic;
using System.IO;
using Tests.model;
using NUnit.Framework;
using Akamai.EdgeGrid;
using System.Net.Http;

namespace Tests
{
    [TestFixture]
    public class EdgeGridSignerHeaderTest
    {
        [Test, TestCaseSource("GetEdgeGridSignerTestFromJson")]
        public void TestEdgeGridSignerHeader(EdgeGridSignerHeader item)
        {
            Assert.NotNull(item);
            Assert.IsFalse(item.Timestamp.IsValid());
            var Authentication = new EdgeGridSigner();
            Authentication.SetCredential(item.Auth);
            var Method = (item.Method == EdgeGridSignerHeaderDataMethod.Method.Get) ? HttpMethod.Get : HttpMethod.Post;
            Authentication.SetHttpMethod(Method);
            Authentication.Timestamp = item.Timestamp;
            Authentication.Nonce = item.Nonce;
            Authentication.SetBodyContent(item.Body);
            Authentication.SetRequestUri(item.Uri.ToString());
            var AuthorizationString = Authentication.GenerateAuthorizationString();

            Assert.AreEqual(item.Expected, AuthorizationString);
        }

        private static IEnumerable<EdgeGridSignerHeader> GetEdgeGridSignerTestFromJson()
        {
            using (var JsonFile = new StreamReader("../../../testdata.json"))
            {
                var Json = JsonFile.ReadToEnd();
                var Collection = EdgeGridSignerHeaderData.FromJson(Json);

                var Credential = new Credential(Collection.ClientToken, Collection.AccessToken,
                    Collection.ClientSecret);
                var Uri = new Uri(Collection.BaseUrl);
                var HeaderToSign = Collection.HeadersToSign;
                var Nonce = new EdgeGridNonce(Collection.Nonce);
                var Timestamp = new EdgeGridTimestamp(Collection.Timestamp);
                var MaxBody = Collection.MaxBody;
                var Items = new List<EdgeGridSignerHeader>();
                foreach (var Item in Collection.Tests)
                {
                    Items.Add(new EdgeGridSignerHeader(Credential, Uri, Item.Request.Headers, HeaderToSign, Nonce,
                        Timestamp, MaxBody, Item.Request.Method, Item.Request.Path, Item.ExpectedAuthorization,
                        Item.Request.Query, "", Item.TestName));
                }

                return Items;
            }
        }
    }
}