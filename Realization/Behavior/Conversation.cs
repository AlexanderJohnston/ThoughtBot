namespace Realization.Converse
{
    public class Conversation
    {
        public bool Focusing;
        public string Topic;
        public ulong Author;
        public List<string> Context = new List<string>();
        public List<Message> Memories = new List<Message>();
        private Mood Outlook;


    }

    public class Message
    {
        public Guid Id { get; set; }
        public ulong Author { get; set; }
    }
}
