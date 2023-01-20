using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory.Converse
{
    public class ConversationalMemory
    {
        public Conversation Conversation { get; set; }
        public List<string> Memories { get; set; }
        public ConversationalMemory(Conversation conversation, List<string> memories)
        {
            Conversation = conversation;
            Memories = memories;
        }
    }
}
