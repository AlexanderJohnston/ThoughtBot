using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Memory.Converse;

namespace Memory
{
    // Stores the GptEmbedding and the text used to embed a query from the user.
    public class QueryEmbedding
    {
        [JsonProperty("Text")]
        public string Text { get; set; }
        [JsonProperty("embedding")]
        public GptEmbedding Embedding { get; set; }
        [JsonProperty("topic")]
        public string Topic { get; set; }
        [JsonProperty("context")]
        public string Context { get; set; }
        public QueryEmbedding(GptEmbedding embedding, string text, string topic, string context)
        {
            Text = text;
            Embedding = embedding;
            Topic = topic;
            Context = context;
        }
    }

    // Stores the GptEmbedding and the text used to embed it.
    public class EmbeddedMemory
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        [JsonProperty("context")]
        public string Context { get; set; }
        [JsonProperty("conversation")]
        public Conversation Conversation { get; set; }
        [JsonProperty("embedding")]
        public GptEmbedding Embedding { get; set; }
        public EmbeddedMemory(GptEmbedding embedding, Conversation conversation, string topic, string context)
        {
            Embedding = embedding;
            Conversation = conversation;
            Topic = topic;
            Context = context;
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