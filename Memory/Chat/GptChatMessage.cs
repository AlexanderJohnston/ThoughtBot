using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory.Chat
{
    public class GptChatMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }

        public GptChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }
}
