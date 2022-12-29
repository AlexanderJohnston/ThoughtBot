using System.Text.Json.Serialization;

namespace Memory
{
    public class EmbeddedData
    {
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("index")]
        public int Index { get; set; }
        [JsonPropertyName("embedding")]
        public double[] Embedding { get; set; }
    }
}