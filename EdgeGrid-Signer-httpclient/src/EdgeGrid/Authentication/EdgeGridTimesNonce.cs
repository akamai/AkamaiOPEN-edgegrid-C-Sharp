using System;

namespace AkamaiEdgeGrid.EdgeGrid
{
    /// <summary>
    /// Generate a nonce.
    /// </summary>
    public class EdgeGridNonce
    {
        protected string nonce;

        public EdgeGridNonce()
        {
            this.nonce = this.GetNonce();
        }

        public EdgeGridNonce(string nonce)
        {
            if (String.IsNullOrWhiteSpace(nonce))
            {
                this.nonce = this.GetNonce();
            }
            else
            {
                this.nonce = nonce;
            }
        }

        public string GetNonce(){
            return Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return this.nonce;
        }
    }
}
