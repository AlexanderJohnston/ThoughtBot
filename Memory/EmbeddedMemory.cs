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
        public ulong Topic { get; set; }
        [JsonProperty("context")]
        public ulong Context { get; set; }
        [JsonProperty("conversation")]
        public Memorable Memory { get; set; }
        [JsonProperty("embedding")]
        public GptEmbedding Embedding { get; set; }
        public EmbeddedMemory(GptEmbedding embedding, string message, string username, ulong userId, ulong topic, ulong context)
        {
            Embedding = embedding;
            Memory = new(message, username, userId);
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
    public class Memorable
    {
        public string Message { get; set; }
        public string Username { get; set; }
        public ulong UserId { get; set; }

        public Memorable(string message, string username, ulong userId)
        {
            Message = message;
            Username = username;
            UserId = userId;
        }
        public string ToString() => $"{Username}: {Message}";
    }
}