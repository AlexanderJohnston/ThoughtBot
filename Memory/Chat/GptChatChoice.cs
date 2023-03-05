using Newtonsoft.Json;

namespace Memory.Chat
{
    public class GptChatChoice
    {
        [JsonProperty("index")]
        public int Index { get; set; }
        [JsonProperty("message")]
        public GptChatMessage Message { get; set; }
        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; } = "text-choice-ada-002-v2";
    }
}