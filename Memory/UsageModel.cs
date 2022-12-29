using System.Text.Json.Serialization;

namespace Memory
{
    public class UsageModel
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens;
        [JsonPropertyName("total_tokens")]
        public int TotalTokens;
    }
}