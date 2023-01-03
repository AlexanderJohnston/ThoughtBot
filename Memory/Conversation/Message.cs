namespace Memory.Conversation
{
    public class Message
    {
        public Message(ulong author, Guid id)
        {
            Id = id;
            Author = author;
        }

        public Guid Id { get; set; }
        public ulong Author { get; set; }
    }
}