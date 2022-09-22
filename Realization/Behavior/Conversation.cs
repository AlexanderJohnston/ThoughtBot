using System;
using System.Collections.Generic;
using System.Text;

namespace Realization.Converse
{
    public class Conversation
    {
        public bool Focusing;
        public string Topic;
        public string Author;
        public List<string> Context = new List<string>();
        public List<string> Memories = new List<string>();
        private Mood Outlook;


    }
}
