namespace OpenApi.Exception
{
    public class OpenApiArgumentException : System.Exception
    {
        public OpenApiArgumentException(string message) : base(message){}
        public OpenApiArgumentException(string message, System.Exception inner): base(message, inner){} 
    }
}