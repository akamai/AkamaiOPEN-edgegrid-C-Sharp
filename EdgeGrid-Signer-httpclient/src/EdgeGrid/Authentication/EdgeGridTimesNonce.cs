using System;

namespace Akamai.EdgeGrid
{
    /// <summary>
    /// Generate a nonce.
    /// </summary>
    public class EdgeGridNonce
    {
        protected string Nonce;

        public EdgeGridNonce()
        {
            Nonce = GetNonce();
        }

        public EdgeGridNonce(string nonce)
        {
            Nonce = String.IsNullOrWhiteSpace(nonce) ? GetNonce() : nonce;
        }

        public string GetNonce(){
            return Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return Nonce;
        }
    }
}
