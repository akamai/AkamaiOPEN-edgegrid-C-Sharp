using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Akamai.EdgeGrid.Auth
{
    /// <summary>
    /// An enum of the hash algorithms supported by <see cref="Auth.Utils.ExtensionMethods.ComputeKeyedHash(Stream, KeyedHashAlgorithm"/>
    /// Currently supported hashes include MD5; SHA1; SHA256
    ///
    /// The string representation matches the <see cref="System.Security.Cryptography.HMAC"/> canonical names.
    /// </summary>
    public enum KeyedHashAlgorithm { HMACSHA256, HMACSHA1, HMACMD5 };

    /// <summary>
    /// An enum of the hash algorithms supported by <see cref="Auth.Utils.ExtensionMethods.ComputeHash(Stream, ChecksumAlgorithm"/>
    /// Currently supported hashes include MD5; SHA1; SHA256
    ///
    /// The string representation matches the <see cref="System.Security.Cryptography.HashAlgorithm"/> canonical names.
    /// </summary>
    public enum ChecksumAlgorithm { SHA256, SHA1, MD5 };

    internal static class HashTypeFactory
    {
        public static HMAC Create(KeyedHashAlgorithm hashType)
        {
            switch (hashType)
            {
                case KeyedHashAlgorithm.HMACSHA256:
                    return new HMACSHA256();
                case KeyedHashAlgorithm.HMACSHA1:
                    return new HMACSHA1();
                case KeyedHashAlgorithm.HMACMD5:
                    return new HMACMD5();
                default:
                    throw new InvalidOperationException($"Invalid key hash type \"{hashType}\".");
            }
        }

        public static HashAlgorithm Create(ChecksumAlgorithm hashType)
        {
            switch (hashType)
            {
                case ChecksumAlgorithm.SHA256:
                    return SHA256.Create();
                case ChecksumAlgorithm.SHA1:
                    return SHA1.Create();
                case ChecksumAlgorithm.MD5:
                    return MD5.Create();
                default:
                    throw new InvalidOperationException($"Invalid key hash type \"{hashType}\".");
            }
        }
    }
}
