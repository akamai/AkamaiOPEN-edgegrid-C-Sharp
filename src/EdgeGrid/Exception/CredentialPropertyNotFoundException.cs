using System;

namespace Akamai.EdgeGrid.Exception
{
    public class CredentialPropertyNotFoundException : System.Exception
    {
        public CredentialPropertyNotFoundException(){}
        public CredentialPropertyNotFoundException(string message) : base(message){}
        public CredentialPropertyNotFoundException(string message, System.Exception inner): base(message, inner){}
    }
}