using System;
using System.Reflection;

namespace OpenApi.Model
{
    public static class OpenApiCommandStringValueExtension
    {
        /// <summary>
        /// String value for Command Enum
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ValueToString(this Enum value)
        {
            // Get the type
            Type Type = value.GetType();

            // Get fieldinfo for this type
            FieldInfo FieldInfo = Type.GetField(value.ToString());

            // Get the stringvalue attributes
            var Attribute = FieldInfo.GetCustomAttributes(
                typeof(OpenApiCommandStringValue), false) as OpenApiCommandStringValue[];

            // Return the first if there was a match.
            return Attribute != null && Attribute.Length > 0 ? Attribute[0].Value : null;
        }
    }
}