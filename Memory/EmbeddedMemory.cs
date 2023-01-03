using Newtonsoft.Json;

namespace Memory
{
    // Stores the GptEmbedding and the text used to embed it.
    public class EmbeddedMemory
    {
        public string Topic { get; set; }
        public string Context { get; set; }
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
        [JsonProperty("object")]
        public string Object { get; set; }
        [JsonProperty("data")]
        public EmbeddedData[] Data { get; set; }
        [JsonProperty("model")]
        public string Model { get; set; } = "text-embedding-ada-002-v2";
        [JsonProperty("usage")]
        public UsageModel Usage { get; set; }
    }
}