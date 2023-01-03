using Newtonsoft.Json;

namespace Memory
{
    public class UsageModel
    {
        [JsonProperty("prompt_tokens")]
        public int PromptTokens;
        [JsonProperty("total_tokens")]
        public int TotalTokens;
    }
}