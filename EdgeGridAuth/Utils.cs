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
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Akamai.Utils
{
    /// <summary>
    /// An enum of the hash algorithms supported by <see cref="#ExtensionMethods.ComputeHash(Stream, ChecksumAlgorithm"/>
    /// Currently supported hashes include MD5; SHA1; SHA256
    ///
    /// The string representation matches the <see cref="System.Security.Cryptography.HashAlgorithm"/> canonical names.
    /// </summary>
    public enum ChecksumAlgorithm { SHA256, SHA1, MD5 };

    /// <summary>
    /// An enum of the hash algorithms supported by <see cref="#ExtensionMethods.ComputeKeyedHash(Stream, KeyedHashAlgorithm"/>
    /// Currently supported hashes include MD5; SHA1; SHA256
    ///
    /// The string representation matches the <see cref="System.Security.Cryptography.HMAC"/> canonical names.
    /// </summary>
    public enum KeyedHashAlgorithm { HMACSHA256, HMACSHA1, HMACMD5 };

    /// <summary>
    /// General utility functions needed to implement the NetStorageKit.  Many of these functions are also
    /// available as standard parts of other libraries, but this package strives to operate without any
    /// external dependencies.
    /// 
    /// Author: colinb@akamai.com  (Colin Bendell)
    /// </summary>
    public static class ExtensionMethods
    {

        /// <summary>
        /// Computes the hash of a given InputStream. This is a wrapper over the HashAlgorithm crypto functions.
        /// </summary>
        /// <param name="stream">the source stream. Use a MemoryStream if uncertain.</param>
        /// <param name="hashType">the Algorithm to use to compute the hash</param>
        /// <returns>a byte[] representation of the hash. If the Stream is a null object 
        /// then null will be returned. If the Stream is empty an empty byte[] {} will be returned.</returns>
        public static byte[] ComputeHash(this Stream stream, ChecksumAlgorithm hashType = ChecksumAlgorithm.SHA256)
        {
            if (stream == null) return null;

            using (var algorithm = HashAlgorithm.Create(hashType.ToString()))
                return algorithm.ComputeHash(stream);
        }

        /// <summary>
        /// Computes the HMAC hash of a given byte[]. This is a wrapper over the Mac crypto functions.
        /// </summary>
        /// <param name="data">byte[] of content to hash</param>
        /// <param name="key">secret key to salt the hash. This is assumed to be UTF-8 encoded</param>
        /// <param name="hashType">determines which alogirthm to use. The recommendation is to use HMAC-SHA256</param>
        /// <returns>a byte[] presenting the HMAC hash of the source data. If the data object is null, null will be returned</returns>
        public static byte[] ComputeKeyedHash(this byte[] data, string key, KeyedHashAlgorithm hashType = KeyedHashAlgorithm.HMACSHA256)
        {
            if (data == null) return null;

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            using (var algorithm = HMAC.Create(hashType.ToString()))
            {
                algorithm.Key = key.ToByteArray();
                return algorithm.ComputeHash(data);
            }
        }

        /// <summary>
        /// Retrieve the byte array for a string in UTF8 encoding
        /// </summary>
        /// <param name="data">the data string</param>
        /// <returns>the UTF8 encoded byte array</returns>
        public static byte[] ToByteArray(this string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        /// <summary>
        /// Base64-encode a byte array.
        /// </summary>
        /// <param name="data">byte array to encode.</param>
        /// <returns>Encoded string.</returns>
        public static string ToBase64(this byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// Hex encoding wrapper for a byte array. The output will be 2 character padded string in lower case.
        /// </summary>
        /// <param name="data">a byte array to encode. The assumption is that the string to encode 
        /// is small enough to be held in memory without streaming the encoding</param>
        /// <returns>a 2 character zero padded string in lower case</returns>
        public static string ToHex(this byte[] data)
        {
            return String.Concat(Array.ConvertAll(data, x => x.ToString("X2"))).ToLower();
        }

        /// <summary>
        /// determine the number of seconds since unix epoch
        /// </summary>
        /// <param name="current">the date and time to convert</param>
        /// <returns>the number of seconds since unix epoch</returns>
        public static long GetEpochSeconds(this DateTime current)
        {
            return (long)current.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// format a date and time into ISO8601 style of "yyyyMMdd'T'HH:mm:ss+0000"
        /// </summary>
        /// <param name="timestamp">the date and time to convert</param>
        /// <returns>the formatted date</returns>
        public static string ToISO8601(this DateTime timestamp)
        {
            return timestamp.ToUniversalTime().ToString("yyyyMMdd'T'HH:mm:ss+0000");
        }

        /// <summary>
        /// a utility method to uri encode a text string
        /// </summary>
        /// <param name="data">the string</param>
        /// <returns>the escaped version</returns>
        public static string URLEncode(this string data)
        {
            return Uri.EscapeDataString(data);
        }
    }
}
