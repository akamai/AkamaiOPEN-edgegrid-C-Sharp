using System;

namespace Akamai.EdgeGrid.Exception
{
    public class PropertyCouldNotBeLoadedException : System.Exception
    {
        public PropertyCouldNotBeLoadedException(){}
        public PropertyCouldNotBeLoadedException(string message) : base(message){}
        public PropertyCouldNotBeLoadedException(string message, System.Exception inner): base(message, inner){}

    }
}