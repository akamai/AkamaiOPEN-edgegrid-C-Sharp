namespace OpenApi.Exception
{
    public class OpenApiCredentialException : System.Exception
    {
        public OpenApiCredentialException(string message) : base(message){}
        public OpenApiCredentialException(string message, System.Exception inner): base(message, inner){} 
    }
}