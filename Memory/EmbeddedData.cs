using Newtonsoft.Json;

namespace Memory
{
    public class EmbeddedData
    {
        [JsonProperty("object")]
        public string Object { get; set; }
        [JsonProperty("index")]
        public int Index { get; set; }
        [JsonProperty("embedding")]
        public double[] Embedding { get; set; }
    }
}