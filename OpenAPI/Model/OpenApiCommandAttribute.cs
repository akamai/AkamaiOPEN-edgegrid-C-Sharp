using System;

namespace OpenApi.Model
{
    public class OpenApiCommandStringValue : Attribute
    {
        public string Value { get; protected set; }

        /// <inheritdoc />
        /// <summary>
        /// Hols string value for the enum command
        /// </summary>
        /// <param name="value">String value for the command</param>
        public OpenApiCommandStringValue(string value)
        {
            Value = value;
        }
    }
}