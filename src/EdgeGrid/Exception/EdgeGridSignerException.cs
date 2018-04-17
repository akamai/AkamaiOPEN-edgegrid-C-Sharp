namespace Akamai.EdgeGrid.Exception
{
    public class EdgeGridSignerException : System.Exception
    {
        public EdgeGridSignerException(){}
        public EdgeGridSignerException(string message) : base(message){}
        public EdgeGridSignerException(string message, System.Exception inner): base(message, inner){} 
    }
}