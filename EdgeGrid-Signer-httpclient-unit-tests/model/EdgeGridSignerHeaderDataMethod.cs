using System;
using Newtonsoft.Json;

namespace Tests.model
{
    public static class EdgeGridSignerHeaderDataMethod
    {
        public enum Method
        {
            Get,
            Post,
            Put
        }

        private static Method? ValueForString(string str)
        {
            switch (str)
            {
                case "GET": return Method.Get;
                case "POST": return Method.Post;
                case "PUT": return Method.Put;
                default: return null;
            }
        }

        public static Method ReadJson(JsonReader reader, JsonSerializer serializer)
        {
            var Str = serializer.Deserialize<string>(reader);
            var MaybeValue = ValueForString(Str);
            if (MaybeValue.HasValue) return MaybeValue.Value;
            throw new Exception("Unknown enum case " + Str);
        }

        public static void WriteJson(this Method value, JsonWriter writer, JsonSerializer serializer)
        {
            switch (value)
            {
                case Method.Get:
                    serializer.Serialize(writer, "GET");
                    break;
                case Method.Post:
                    serializer.Serialize(writer, "POST");
                    break;
                case Method.Put:
                    serializer.Serialize(writer, "PUT");
                    break;
            }
        }
    }
}