using System.Text.Json.Serialization;

namespace Realization.Skill
{
    public class UsageModel
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens;
        [JsonPropertyName("total_tokens")]
        public int TotalTokens;
    }
}