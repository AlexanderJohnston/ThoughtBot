using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory.Chat
{
    public class GptChatRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = "gpt-3.5-turbo";
        [JsonProperty("messages")]
        public List<GptChatMessage> Messages { get; set; }
        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; } = 256;
        [JsonProperty("temperature")]
        public double Temperature { get; set; } = 0.7f;
        [JsonProperty("n")]
        public int N { get; set; } = 1;
        [JsonProperty("user")]
        public string User { get; set; } = "System";
    }
}
