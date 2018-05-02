using Newtonsoft.Json;

namespace Tests.model
{
    public class EdgeGridSignerHeaderDataTests
    {
        [JsonProperty("testName", NullValueHandling = NullValueHandling.Ignore)]
        public string TestName { get; set; }

        [JsonProperty("request", NullValueHandling = NullValueHandling.Ignore)]
        public EdgeGridSignerHeaderDataRequest Request { get; set; }

        [JsonProperty("expectedAuthorization", NullValueHandling = NullValueHandling.Ignore)]
        public string ExpectedAuthorization { get; set; }
    }
}