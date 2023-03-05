using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory.Chat
{
    public class GptChatResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("object")]
        public string Object { get; set; }
        [JsonProperty("created")]
        public ulong Created { get; set; }
        [JsonProperty("choices")]
        public List<GptChatChoice> Choices { get; set; }
        [JsonProperty("usage")]
        public GptUsage Usage { get; set; }
    }
}
