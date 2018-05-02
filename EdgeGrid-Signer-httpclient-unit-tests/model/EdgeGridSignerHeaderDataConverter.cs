using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Tests.model
{
    internal class EdgeGridSignerHeaderDataConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(EdgeGridSignerHeaderDataMethod.Method) ||
                                                   t == typeof(EdgeGridSignerHeaderDataMethod.Method?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (t == typeof(EdgeGridSignerHeaderDataMethod.Method))
                return EdgeGridSignerHeaderDataMethod.ReadJson(reader, serializer);
            if (t == typeof(EdgeGridSignerHeaderDataMethod.Method?))
            {
                if (reader.TokenType == JsonToken.Null) return null;
                return EdgeGridSignerHeaderDataMethod.ReadJson(reader, serializer);
            }

            throw new Exception("Unknown type");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var TypeForValue = value.GetType();
            if (TypeForValue == typeof(EdgeGridSignerHeaderDataMethod.Method))
            {
                ((EdgeGridSignerHeaderDataMethod.Method) value).WriteJson(writer, serializer);
                return;
            }

            throw new Exception("Unknown type");
        }

        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new EdgeGridSignerHeaderDataConverter(),
                new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.AssumeUniversal}
            }
        };
    }
}