using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tests.model
{
    public class EdgeGridSignerHeaderDataRequest
    {
        [JsonProperty("method", NullValueHandling = NullValueHandling.Ignore)]
        public EdgeGridSignerHeaderDataMethod.Method? Method { get; set; }

        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }

        [JsonProperty("headers", NullValueHandling = NullValueHandling.Ignore)]
        public List<Dictionary<string, string>> Headers { get; set; }

        [JsonProperty("data")] public string Data { get; set; }

        [JsonProperty("query", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Query { get; set; }
    }
}