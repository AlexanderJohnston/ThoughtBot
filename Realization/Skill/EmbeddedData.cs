using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Realization.Skill
{
    public class EmbeddedData
    {
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("index")]
        public int Index { get; set; }
        [JsonPropertyName("embedding")]
        public Double[] Embedding { get; set; }
    }
}