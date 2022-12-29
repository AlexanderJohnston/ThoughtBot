using System.Text.Json.Serialization;

namespace Memory
{
    // Stores the GptEmbedding and the text used to embed it.
    public class EmbeddedMemory
    {
        public string Text { get; set; }
        public GptEmbedding Embedding { get; set; }
        public EmbeddedMemory(GptEmbedding embedding, string text)
        {
            Embedding = embedding;
            Text = text;
        }
    }
    public class GptEmbedding
    {
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("data")]
        public EmbeddedData[] Data { get; set; }
        [JsonPropertyName("model")]
        public string Model { get; set; } = "text-embedding-ada-002-v2";
        [JsonPropertyName("usage")]
        public UsageModel Usage { get; set; }
    }
}