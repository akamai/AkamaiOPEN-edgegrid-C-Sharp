using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Tests.model
{
    public class EdgeGridSignerHeaderData
    {
        [JsonProperty("base_url", NullValueHandling = NullValueHandling.Ignore)]
        public string BaseUrl { get; set; }

        [JsonProperty("access_token", NullValueHandling = NullValueHandling.Ignore)]
        public string AccessToken { get; set; }

        [JsonProperty("client_token", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientToken { get; set; }

        [JsonProperty("client_secret", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientSecret { get; set; }

        [JsonProperty("max_body", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxBody { get; set; }

        [JsonProperty("headers_to_sign", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> HeadersToSign { get; set; }

        [JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
        public string Nonce { get; set; }

        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public string Timestamp { get; set; }

        [JsonProperty("tests", NullValueHandling = NullValueHandling.Ignore)]
        public List<EdgeGridSignerHeaderDataTests> Tests { get; set; }

        public static EdgeGridSignerHeaderData FromJson(string json) =>
            JsonConvert.DeserializeObject<EdgeGridSignerHeaderData>(json, EdgeGridSignerHeaderDataConverter.Settings);
    }
}