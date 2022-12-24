using System.Text.Json.Serialization;

namespace Realization.Skill
{
    public class EmbeddedMemory
    {
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("data")]
        public EmbeddedData[] Data { get; set; }
        [JsonPropertyName("model")]
        public string Model { get; set; } = "text-embedding-ada-002-v2";
        [JsonPropertyName("usage")]
        public UsageModel Usage {get; set; }
    }
}