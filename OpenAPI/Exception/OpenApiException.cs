namespace OpenApi.Exception
{
    public class OpenApiException : System.Exception
    {
        public OpenApiException(string message) : base(message){}
        public OpenApiException(string message, System.Exception inner): base(message, inner){} 
    }
}