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
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akamai.Utils
{
    [TestClass]
    public class UtilsTest
    {
        [TestMethod]
        public void ComputeHashNullTest()
        {
            Assert.IsNull(ExtensionMethods.ComputeHash(null));
        }

        [TestMethod]
        public void ComputeHashHexTest()
        {
            var data = "Lorem ipsum dolor sit amet, an sea putant quaeque, homero aperiam te eos.".ToByteArray();
            Assert.AreEqual(new MemoryStream(data).ComputeHash(ChecksumAlgorithm.SHA256).ToHex().ToLower(),
                "4e8aecd6dc4c97ae55c30ef9b1e91b4829ef5871b16262b4628838a80dc0c2e2");
            Assert.AreEqual(new MemoryStream(data).ComputeHash(ChecksumAlgorithm.SHA1).ToHex().ToLower(),
                "5efe96a4d243965e4edd3142d5ee061ab2f57055");
            Assert.AreEqual(new MemoryStream(data).ComputeHash(ChecksumAlgorithm.MD5).ToHex().ToLower(),
                "7d5efa77cfaaff5f18001612b426fe36");
        }

        [TestMethod]
        public void ComputeHashBase64LimitTest()
        {
            var data = "Lorem ipsum dolor sit amet, an sea putant quaeque, homero aperiam te eos.".ToByteArray();
            Assert.AreEqual(new MemoryStream(data).ComputeHash(ChecksumAlgorithm.SHA256, 50).ToBase64(),
                "IHJu55sckdViGcpD7CpUttVSzYoy/DiTQsmy7jrzoMU=");
        }

        [TestMethod]
        public void ComputeHashBase64Test()
        {
            var data = "Lorem ipsum dolor sit amet, an sea putant quaeque, homero aperiam te eos.".ToByteArray();

            Assert.AreEqual(new MemoryStream(data).ComputeHash(ChecksumAlgorithm.SHA256).ToBase64(),
                "Tors1txMl65Vww75sekbSCnvWHGxYmK0Yog4qA3AwuI=");
            Assert.AreEqual(new MemoryStream(data).ComputeHash(ChecksumAlgorithm.SHA1).ToBase64(),
                "Xv6WpNJDll5O3TFC1e4GGrL1cFU=");
            Assert.AreEqual(new MemoryStream(data).ComputeHash(ChecksumAlgorithm.MD5).ToBase64(),
                "fV76d8+q/18YABYStCb+Ng==");
        }

        [TestMethod]
        public void ComputeKeyedHashNullTest()
        {
            Assert.IsNull(ExtensionMethods.ComputeKeyedHash(null, "secretkey"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ComputeKeyedHashNullException()
        {
            ExtensionMethods.ComputeKeyedHash("value".ToByteArray(), null);
        }

        [TestMethod]
        public void ComputeKeyedHashTest()
        {
            var data = "Lorem ipsum dolor sit amet, an sea putant quaeque, homero aperiam te eos.".ToByteArray();
            var key = "secretkey";

            Assert.AreEqual(data.ComputeKeyedHash(key, KeyedHashAlgorithm.HMACSHA256).ToBase64(),
                "+jYoZtNP2pVjCx/cMWWM+NCe1kpTW7y1mnM7zi5tr6c=");
            Assert.AreEqual(data.ComputeKeyedHash(key, KeyedHashAlgorithm.HMACSHA1).ToBase64(),
                "eXk3UE/WTxvyh9RW8fLNJtwF9j4=");
            Assert.AreEqual(data.ComputeKeyedHash(key, KeyedHashAlgorithm.HMACMD5).ToBase64(),
                "kTFtTrQ9vdBM5/97A5kPIQ==");
        }

        [TestMethod]
        public void ToBase64Test()
        {
            var data = "Lorem ipsum dolor sit amet";
            Assert.AreEqual(data.ToByteArray().ToBase64(), "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQ=");
        }

        [TestMethod]
        public void ToHexTest()
        {
            var data = "Lorem ipsum dolor sit amet";
            Assert.AreEqual(data.ToByteArray().ToHex(), "4c6f72656d20697073756d20646f6c6f722073697420616d6574");
        }

        [TestMethod]
        public void TestGetEpochSeconds()
        {
            Assert.AreEqual(new DateTime(2013, 11, 11, 0, 0, 0, DateTimeKind.Utc).GetEpochSeconds(), 1384128000);
        }

        [TestMethod]
        public void ToISO8601Test()
        {
            var timestamp = new DateTime(1918, 11, 11, 11, 00, 00, DateTimeKind.Utc);
            Assert.AreEqual(timestamp.ToISO8601(), "19181111T11:00:00+0000");
        }

        [TestMethod]
        public void URLEncodeTest()
        {
            // this is probably not a complete test, should be expanded...
            var data = "Lorem ipsum";
            Assert.AreEqual(data.URLEncode(), "Lorem%20ipsum");
        }

        [TestMethod]
        public void ReadExactlyTest()
        {
            var data = "Lorem ipsum dolor sit amet, an sea putant quaeque, homero aperiam te eos.".ToByteArray();
            using (MemoryStream stream = new MemoryStream(data))
            {
                Assert.AreEqual(Encoding.UTF8.GetString(stream.ReadExactly(50)), "Lorem ipsum dolor sit amet, an sea putant quaeque,");
            }
        }
    }
}
